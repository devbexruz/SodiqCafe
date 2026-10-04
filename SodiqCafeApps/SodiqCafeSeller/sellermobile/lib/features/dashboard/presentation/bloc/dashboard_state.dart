part of 'dashboard_cubit.dart';

abstract class DashboardState extends Equatable {
  const DashboardState();

  @override
  List<Object> get props => [];
}

class DashboardInitial extends DashboardState {}

class DashboardLoading extends DashboardState {}

class DashboardLoaded extends DashboardState {
  final LocalCafe cafes;
  final List<dynamic> allCafes;
  final int todayServed;
  final int totalCustomers;
  final int totalProducts;
  final int totalBonusesGiven;
  final int daysLeft;
  final int balance;
  final int basePrice;

  const DashboardLoaded({
    required this.cafes,
    required this.allCafes,
    required this.todayServed,
    required this.totalCustomers,
    required this.totalProducts,
    required this.totalBonusesGiven,
    required this.daysLeft,
    required this.balance,
    required this.basePrice,
  });

  @override
  List<Object> get props => [
        cafes,
        allCafes,
        todayServed,
        totalCustomers,
        totalProducts,
        totalBonusesGiven,
        daysLeft,
        balance,
        basePrice,
      ];
}

class DashboardError extends DashboardState {
  final String message;

  const DashboardError({required this.message});

  @override
  List<Object> get props => [message];
}
