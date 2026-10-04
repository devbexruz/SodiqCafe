import 'package:drift/drift.dart';

class LocalCafes extends Table {
  IntColumn get id => integer()();
  TextColumn get name => text()();
  BoolColumn get isCurrent => boolean().withDefault(const Constant(false))();
  
  @override
  Set<Column> get primaryKey => {id};
}
