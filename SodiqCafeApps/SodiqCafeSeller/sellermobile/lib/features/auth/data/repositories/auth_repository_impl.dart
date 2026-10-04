import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/repositories/auth_repository.dart';
import '../models/login_response.dart';

class AuthRepositoryImpl implements AuthRepository {
  final ApiClient apiClient;
  final SharedPreferences sharedPreferences;

  AuthRepositoryImpl({required this.apiClient, required this.sharedPreferences});

  static const String tokenKey = 'ACCESS_TOKEN';
  static const String sessionKey = 'SESSION_TOKEN';

  @override
  Future<Either<String, LoginResponse>> login(String usernameOrEmail, String password) async {
    try {
      final response = await apiClient.dio.post('/auth/login', data: {
        'usernameOrEmail': usernameOrEmail,
        'password': password,
        'rememberMe': true,
      });

      final loginResponse = LoginResponse.fromJson(response.data);

      if (loginResponse.accessToken != null) {
        await sharedPreferences.setString(tokenKey, loginResponse.accessToken!);
      }
      if (loginResponse.sessionToken != null) {
        await sharedPreferences.setString(sessionKey, loginResponse.sessionToken!);
      }
      if (loginResponse.refreshToken != null) {
        await sharedPreferences.setString('REFRESH_TOKEN', loginResponse.refreshToken!);
      }

      return Right(loginResponse);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Login xatosi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<void> logout() async {
    await sharedPreferences.remove(tokenKey);
    await sharedPreferences.remove(sessionKey);
    await sharedPreferences.remove('REFRESH_TOKEN');
  }

  @override
  Future<bool> isLoggedIn() async {
    final token = sharedPreferences.getString(tokenKey);
    return token != null && token.isNotEmpty;
  }

  @override
  Future<String?> getToken() async {
    return sharedPreferences.getString(tokenKey);
  }
}
