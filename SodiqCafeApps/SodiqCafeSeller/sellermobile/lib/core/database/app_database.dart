import 'dart:io';
import 'package:drift/drift.dart';
import 'package:drift/native.dart';
import 'package:path_provider/path_provider.dart';
import 'package:path/path.dart' as p;

import 'tables.dart';

part 'app_database.g.dart';

@DriftDatabase(tables: [LocalCafes])
class AppDatabase extends _$AppDatabase {
  AppDatabase() : super(_openConnection());

  @override
  int get schemaVersion => 1;

  Future<void> saveCafes(List<LocalCafesCompanion> cafes) async {
    await transaction(() async {
      await delete(localCafes).go();
      for (final cafe in cafes) {
        await into(localCafes).insert(cafe);
      }
    });
  }

  Future<void> setCurrentCafe(int id) async {
    await transaction(() async {
      await update(localCafes).write(const LocalCafesCompanion(isCurrent: Value(false)));
      await (update(localCafes)..where((t) => t.id.equals(id)))
          .write(const LocalCafesCompanion(isCurrent: Value(true)));
    });
  }

  Future<LocalCafe?> getCurrentCafe() async {
    return await (select(localCafes)..where((t) => t.isCurrent.equals(true))).getSingleOrNull();
  }
}

LazyDatabase _openConnection() {
  return LazyDatabase(() async {
    final dbFolder = await getApplicationDocumentsDirectory();
    final file = File(p.join(dbFolder.path, 'sodiq_cafe.sqlite'));
    return NativeDatabase.createInBackground(file);
  });
}
