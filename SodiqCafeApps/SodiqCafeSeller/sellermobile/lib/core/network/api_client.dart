import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  final Dio dio;
  final SharedPreferences sharedPreferences;
  bool _isRefreshing = false;

  ApiClient({required this.dio, required this.sharedPreferences}) {
    dio.options.baseUrl = 'https://sodiqcafe.uz/api/v1';
    dio.options.connectTimeout = const Duration(seconds: 10);
    dio.options.receiveTimeout = const Duration(seconds: 10);

    dio.interceptors.add(LogInterceptor(
      request: true,
      requestHeader: true,
      requestBody: true,
      responseHeader: true,
      responseBody: true,
      error: true,
    ));
    
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = sharedPreferences.getString('ACCESS_TOKEN');
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (DioException e, handler) async {
        if (e.response?.statusCode == 401) {
          // Prevent infinite loops if refresh itself fails
          if (e.requestOptions.path.contains('/auth/refresh-token') || e.requestOptions.path.contains('/auth/login')) {
            return handler.next(e);
          }

          if (!_isRefreshing) {
            _isRefreshing = true;
            try {
              final sessionToken = sharedPreferences.getString('SESSION_TOKEN');
              final refreshToken = sharedPreferences.getString('REFRESH_TOKEN');

              if (sessionToken != null && refreshToken != null) {
                // Using a separate dio instance to avoid interceptor infinite loops
                final refreshDio = Dio(BaseOptions(baseUrl: dio.options.baseUrl));
                final response = await refreshDio.post('/auth/refresh-token', data: {
                  'sessionToken': sessionToken,
                  'refreshToken': refreshToken,
                });

                if (response.statusCode == 200) {
                  final data = response.data['data'] ?? response.data;
                  final newAccessToken = data['accessToken'];
                  final newRefreshToken = data['refreshToken'];
                  final newSessionToken = data['sessionToken'];

                  if (newAccessToken != null) {
                    await sharedPreferences.setString('ACCESS_TOKEN', newAccessToken);
                    if (newRefreshToken != null) await sharedPreferences.setString('REFRESH_TOKEN', newRefreshToken);
                    if (newSessionToken != null) await sharedPreferences.setString('SESSION_TOKEN', newSessionToken);
                    
                    // Update header for original request and retry
                    e.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';
                    final retryResponse = await refreshDio.fetch(e.requestOptions);
                    _isRefreshing = false;
                    return handler.resolve(retryResponse);
                  }
                }
              }
            } catch (refreshError) {
              // Refresh failed, proceed to throw 401 error so UI logs out or prompts user
            } finally {
              _isRefreshing = false;
            }
          } else {
            // If already refreshing, wait and retry. For simplicity here, we just throw 401. 
            // In a production app, you might queue requests.
          }
        }
        return handler.next(e);
      }
    ));
  }
}
