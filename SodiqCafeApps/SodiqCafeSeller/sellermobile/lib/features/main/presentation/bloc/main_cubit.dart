import 'package:flutter_bloc/flutter_bloc.dart';

class MainCubit extends Cubit<int> {
  MainCubit() : super(0);

  void changePage(int index) => emit(index);
}
