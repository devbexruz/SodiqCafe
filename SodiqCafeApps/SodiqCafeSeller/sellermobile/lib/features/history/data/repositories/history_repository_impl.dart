import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/repositories/history_repository.dart';

class HistoryRepositoryImpl implements HistoryRepository {
  final ApiClient apiClient;

  HistoryRepositoryImpl({required this.apiClient});

  @override
  Future<Either<String, Map<String, dynamic>>> getHistory(int cafeId) async {
    try {
      final response = await apiClient.dio.get('/owner/history?cafeId=$cafeId');
      return Right(response.data['data'] ?? response.data);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Xatolik yuz berdi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }
}
