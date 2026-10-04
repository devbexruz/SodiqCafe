// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_database.dart';

// ignore_for_file: type=lint
class $LocalCafesTable extends LocalCafes
    with TableInfo<$LocalCafesTable, LocalCafe> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $LocalCafesTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<int> id = GeneratedColumn<int>(
    'id',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _isCurrentMeta = const VerificationMeta(
    'isCurrent',
  );
  @override
  late final GeneratedColumn<bool> isCurrent = GeneratedColumn<bool>(
    'is_current',
    aliasedName,
    false,
    type: DriftSqlType.bool,
    requiredDuringInsert: false,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'CHECK ("is_current" IN (0, 1))',
    ),
    defaultValue: const Constant(false),
  );
  @override
  List<GeneratedColumn> get $columns => [id, name, isCurrent];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'local_cafes';
  @override
  VerificationContext validateIntegrity(
    Insertable<LocalCafe> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('is_current')) {
      context.handle(
        _isCurrentMeta,
        isCurrent.isAcceptableOrUnknown(data['is_current']!, _isCurrentMeta),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  LocalCafe map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return LocalCafe(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      isCurrent: attachedDatabase.typeMapping.read(
        DriftSqlType.bool,
        data['${effectivePrefix}is_current'],
      )!,
    );
  }

  @override
  $LocalCafesTable createAlias(String alias) {
    return $LocalCafesTable(attachedDatabase, alias);
  }
}

class LocalCafe extends DataClass implements Insertable<LocalCafe> {
  final int id;
  final String name;
  final bool isCurrent;
  const LocalCafe({
    required this.id,
    required this.name,
    required this.isCurrent,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<int>(id);
    map['name'] = Variable<String>(name);
    map['is_current'] = Variable<bool>(isCurrent);
    return map;
  }

  LocalCafesCompanion toCompanion(bool nullToAbsent) {
    return LocalCafesCompanion(
      id: Value(id),
      name: Value(name),
      isCurrent: Value(isCurrent),
    );
  }

  factory LocalCafe.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return LocalCafe(
      id: serializer.fromJson<int>(json['id']),
      name: serializer.fromJson<String>(json['name']),
      isCurrent: serializer.fromJson<bool>(json['isCurrent']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<int>(id),
      'name': serializer.toJson<String>(name),
      'isCurrent': serializer.toJson<bool>(isCurrent),
    };
  }

  LocalCafe copyWith({int? id, String? name, bool? isCurrent}) => LocalCafe(
    id: id ?? this.id,
    name: name ?? this.name,
    isCurrent: isCurrent ?? this.isCurrent,
  );
  LocalCafe copyWithCompanion(LocalCafesCompanion data) {
    return LocalCafe(
      id: data.id.present ? data.id.value : this.id,
      name: data.name.present ? data.name.value : this.name,
      isCurrent: data.isCurrent.present ? data.isCurrent.value : this.isCurrent,
    );
  }

  @override
  String toString() {
    return (StringBuffer('LocalCafe(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('isCurrent: $isCurrent')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(id, name, isCurrent);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is LocalCafe &&
          other.id == this.id &&
          other.name == this.name &&
          other.isCurrent == this.isCurrent);
}

class LocalCafesCompanion extends UpdateCompanion<LocalCafe> {
  final Value<int> id;
  final Value<String> name;
  final Value<bool> isCurrent;
  const LocalCafesCompanion({
    this.id = const Value.absent(),
    this.name = const Value.absent(),
    this.isCurrent = const Value.absent(),
  });
  LocalCafesCompanion.insert({
    this.id = const Value.absent(),
    required String name,
    this.isCurrent = const Value.absent(),
  }) : name = Value(name);
  static Insertable<LocalCafe> custom({
    Expression<int>? id,
    Expression<String>? name,
    Expression<bool>? isCurrent,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (name != null) 'name': name,
      if (isCurrent != null) 'is_current': isCurrent,
    });
  }

  LocalCafesCompanion copyWith({
    Value<int>? id,
    Value<String>? name,
    Value<bool>? isCurrent,
  }) {
    return LocalCafesCompanion(
      id: id ?? this.id,
      name: name ?? this.name,
      isCurrent: isCurrent ?? this.isCurrent,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<int>(id.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (isCurrent.present) {
      map['is_current'] = Variable<bool>(isCurrent.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('LocalCafesCompanion(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('isCurrent: $isCurrent')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $LocalCafesTable localCafes = $LocalCafesTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [localCafes];
}

typedef $$LocalCafesTableCreateCompanionBuilder = LocalCafesCompanion Function({
  Value<int> id,
  required String name,
  Value<bool> isCurrent,
});
typedef $$LocalCafesTableUpdateCompanionBuilder = LocalCafesCompanion Function({
  Value<int> id,
  Value<String> name,
  Value<bool> isCurrent,
});

class $$LocalCafesTableFilterComposer
    extends Composer<_$AppDatabase, $LocalCafesTable> {
  $$LocalCafesTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<bool> get isCurrent => $composableBuilder(
    column: $table.isCurrent,
    builder: (column) => ColumnFilters(column),
  );
}

class $$LocalCafesTableOrderingComposer
    extends Composer<_$AppDatabase, $LocalCafesTable> {
  $$LocalCafesTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<bool> get isCurrent => $composableBuilder(
    column: $table.isCurrent,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$LocalCafesTableAnnotationComposer
    extends Composer<_$AppDatabase, $LocalCafesTable> {
  $$LocalCafesTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<int> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<bool> get isCurrent =>
      $composableBuilder(column: $table.isCurrent, builder: (column) => column);
}

class $$LocalCafesTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $LocalCafesTable,
          LocalCafe,
          $$LocalCafesTableFilterComposer,
          $$LocalCafesTableOrderingComposer,
          $$LocalCafesTableAnnotationComposer,
          $$LocalCafesTableCreateCompanionBuilder,
          $$LocalCafesTableUpdateCompanionBuilder,
          (
            LocalCafe,
            BaseReferences<_$AppDatabase, $LocalCafesTable, LocalCafe>,
          ),
          LocalCafe,
          PrefetchHooks Function()
        > {
  $$LocalCafesTableTableManager(_$AppDatabase db, $LocalCafesTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$LocalCafesTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$LocalCafesTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$LocalCafesTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback: ({
            Value<int> id = const Value.absent(),
            Value<String> name = const Value.absent(),
            Value<bool> isCurrent = const Value.absent(),
          }) => LocalCafesCompanion(id: id, name: name, isCurrent: isCurrent),
          createCompanionCallback:
              ({
                Value<int> id = const Value.absent(),
                required String name,
                Value<bool> isCurrent = const Value.absent(),
              }) => LocalCafesCompanion.insert(
                id: id,
                name: name,
                isCurrent: isCurrent,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable<$LocalCafesTable, LocalCafe>(table),
                  BaseReferences<_$AppDatabase, $LocalCafesTable, LocalCafe>(
                    db,
                    table,
                    e,
                  ),
                ),
              )
              .toList(),
          prefetchHooksCallback: null,
        ),
      );
}

typedef $$LocalCafesTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $LocalCafesTable,
      LocalCafe,
      $$LocalCafesTableFilterComposer,
      $$LocalCafesTableOrderingComposer,
      $$LocalCafesTableAnnotationComposer,
      $$LocalCafesTableCreateCompanionBuilder,
      $$LocalCafesTableUpdateCompanionBuilder,
      (LocalCafe, BaseReferences<_$AppDatabase, $LocalCafesTable, LocalCafe>),
      LocalCafe,
      PrefetchHooks Function()
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$LocalCafesTableTableManager get localCafes =>
      $$LocalCafesTableTableManager(_db, _db.localCafes);
}
