import 'package:get_it/get_it.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:dio/dio.dart';
import '../core/network/api_client.dart';

import '../features/auth/domain/repositories/auth_repository.dart';
import '../features/auth/data/repositories/auth_repository_impl.dart';
import '../features/auth/presentation/bloc/auth_bloc.dart';

import '../features/menu/domain/repositories/menu_repository.dart';
import '../features/menu/data/repositories/menu_repository_impl.dart';
import '../features/menu/presentation/bloc/menu_bloc.dart';

import '../core/database/app_database.dart';
import '../features/dashboard/presentation/bloc/dashboard_cubit.dart';

import '../features/history/domain/repositories/history_repository.dart';
import '../features/history/data/repositories/history_repository_impl.dart';
import '../features/history/presentation/bloc/history_cubit.dart';

final sl = GetIt.instance;

Future<void> init() async {
  // --- External ---
  final sharedPreferences = await SharedPreferences.getInstance();
  sl.registerLazySingleton(() => sharedPreferences);
  sl.registerLazySingleton(() => Dio());

  // --- Core ---
  sl.registerLazySingleton(() => ApiClient(dio: sl(), sharedPreferences: sl()));

  // --- Features: Auth ---
  sl.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(apiClient: sl(), sharedPreferences: sl()),
  );
  sl.registerFactory(() => AuthBloc(authRepository: sl()));

  // --- Features: Menu ---
  sl.registerLazySingleton<MenuRepository>(
    () => MenuRepositoryImpl(apiClient: sl()),
  );
  sl.registerFactory(() => MenuBloc(menuRepository: sl()));

  // --- Features: Dashboard ---
  sl.registerLazySingleton(() => AppDatabase());
  sl.registerFactory(() => DashboardCubit(dio: sl<ApiClient>().dio, database: sl()));

  // --- Features: History ---
  sl.registerLazySingleton<HistoryRepository>(
    () => HistoryRepositoryImpl(apiClient: sl()),
  );
  sl.registerFactory(() => HistoryCubit(historyRepository: sl()));
}
