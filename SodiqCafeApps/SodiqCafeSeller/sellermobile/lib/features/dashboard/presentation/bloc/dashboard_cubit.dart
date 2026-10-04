import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:dio/dio.dart';
import '../../../../core/database/app_database.dart';
import 'package:drift/drift.dart' as drift;

part 'dashboard_state.dart';

class DashboardCubit extends Cubit<DashboardState> {
  final Dio dio;
  final AppDatabase database;

  DashboardCubit({required this.dio, required this.database}) : super(DashboardInitial());

  Future<void> loadDashboard() async {
    emit(DashboardLoading());
    try {
      // 1. Fetch available cafes
      final cafesResponse = await dio.get('/owner/cafes');
      final List cafesData = cafesResponse.data['data'] ?? [];
      
      if (cafesData.isEmpty) {
        emit(const DashboardError(message: "Sizda kafelar yo'q"));
        return;
      }

      // Sync with Drift
      final List<LocalCafesCompanion> localCafes = cafesData.map((e) => LocalCafesCompanion(
        id: drift.Value(e['id']),
        name: drift.Value(e['name']),
      )).toList();
      
      // 2. Preserve current cafe if it exists
      var currentCafe = await database.getCurrentCafe();
      int? previousId = currentCafe?.id;

      await database.saveCafes(localCafes);

      if (previousId != null && cafesData.any((e) => e['id'] == previousId)) {
        await database.setCurrentCafe(previousId);
      } else {
        await database.setCurrentCafe(cafesData.first['id']);
      }
      
      currentCafe = await database.getCurrentCafe();

      // 3. Fetch dashboard stats for current cafe
      final statsResponse = await dio.get('/owner/dashboard', queryParameters: {'cafeId': currentCafe!.id});
      final statsData = statsResponse.data['data'];

      emit(DashboardLoaded(
        cafes: currentCafe,
        allCafes: cafesData,
        todayServed: (statsData['todayServed'] as num?)?.toInt() ?? 0,
        totalCustomers: (statsData['totalCustomers'] as num?)?.toInt() ?? 0,
        totalProducts: (statsData['totalProducts'] as num?)?.toInt() ?? 0,
        totalBonusesGiven: (statsData['totalBonusesGiven'] as num?)?.toInt() ?? 0,
        daysLeft: (statsData['daysLeft'] as num?)?.toInt() ?? 0,
        balance: (statsData['balance'] as num?)?.toInt() ?? 0,
        basePrice: (statsData['basePrice'] as num?)?.toInt() ?? 0,
      ));
    } catch (e) {
      if (e is DioException && e.response?.statusCode == 401) {
        emit(const DashboardError(message: "Sessiya vaqti tugagan yoki token topilmadi. Iltimos, tizimga qayta kiring. (Sozlamalar -> Chiqish)"));
      } else {
        emit(DashboardError(message: e.toString()));
      }
    }
  }

  Future<void> switchCafe(int newCafeId) async {
    emit(DashboardLoading());
    try {
      await database.setCurrentCafe(newCafeId);
      await loadDashboard();
    } catch (e) {
      emit(DashboardError(message: e.toString()));
    }
  }
}
