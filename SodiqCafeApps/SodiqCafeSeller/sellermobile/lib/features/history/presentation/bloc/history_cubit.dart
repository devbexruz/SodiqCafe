import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../domain/repositories/history_repository.dart';

// --- States ---
abstract class HistoryState extends Equatable {
  const HistoryState();
  @override
  List<Object> get props => [];
}

class HistoryInitial extends HistoryState {}

class HistoryLoading extends HistoryState {}

class HistoryLoaded extends HistoryState {
  final List<dynamic> history;
  final List<dynamic> rewards;

  const HistoryLoaded({required this.history, required this.rewards});
  @override
  List<Object> get props => [history, rewards];
}

class HistoryError extends HistoryState {
  final String message;
  const HistoryError({required this.message});
  @override
  List<Object> get props => [message];
}

// --- Cubit ---
class HistoryCubit extends Cubit<HistoryState> {
  final HistoryRepository historyRepository;

  HistoryCubit({required this.historyRepository}) : super(HistoryInitial());

  Future<void> loadHistory(int cafeId) async {
    emit(HistoryLoading());
    final result = await historyRepository.getHistory(cafeId);
    
    result.fold(
      (failure) => emit(HistoryError(message: failure)),
      (data) {
        final history = data['history'] ?? [];
        final rewards = data['rewards'] ?? [];
        emit(HistoryLoaded(history: history, rewards: rewards));
      },
    );
  }
}
