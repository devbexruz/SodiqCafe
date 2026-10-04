import 'package:dartz/dartz.dart';

abstract class HistoryRepository {
  Future<Either<String, Map<String, dynamic>>> getHistory(int cafeId);
}
