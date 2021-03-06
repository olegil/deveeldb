﻿// 
//  Copyright 2010-2016 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Linq;

using Deveel.Data.Sql.Tables;
using Deveel.Data.Sql.Types;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Sequences {
	class SequenceTableContainer : ITableContainer {
		private readonly ITransaction transaction;

		private static readonly Field OneValue = Field.Integer(1);


		public SequenceTableContainer(ITransaction transaction) {
			this.transaction = transaction;
		}

		private SequenceManager Manager {
			get {
				var manager = transaction.GetObjectManager(DbObjectType.Sequence) as SequenceManager;
				if (manager == null)
					throw new InvalidOperationException("Invalid sequence manager.");

				return manager;
			}
		}

		public int TableCount {
			get {
				var table = transaction.GetTable(SequenceManager.SequenceTableName);
				return table != null ? table.RowCount : 0;
			}
		}

		private static TableInfo CreateTableInfo(ObjectName schema, string name) {
			var info = new TableInfo(new ObjectName(schema, name));
			info.AddColumn("last_value", PrimitiveTypes.Numeric());
			info.AddColumn("current_value", PrimitiveTypes.Numeric());
			info.AddColumn("top_value", PrimitiveTypes.Numeric());
			info.AddColumn("increment_by", PrimitiveTypes.Numeric());
			info.AddColumn("min_value", PrimitiveTypes.Numeric());
			info.AddColumn("max_value", PrimitiveTypes.Numeric());
			info.AddColumn("start", PrimitiveTypes.Numeric());
			info.AddColumn("cache", PrimitiveTypes.Numeric());
			info.AddColumn("cycle", PrimitiveTypes.Boolean());
			info = info.AsReadOnly();
			return info;
		}

		public int FindByName(ObjectName tableName) {
			if (tableName == null)
				throw new ArgumentNullException("tableName");

			if (tableName.Parent == null)
				return -1;

			var seqInfo = SequenceManager.SequenceInfoTableName;
			if (transaction.RealTableExists(seqInfo)) {
				// Search the table.
				var table = transaction.GetTable(seqInfo);
				var name = Field.VarChar(tableName.Name);
				var schema = Field.VarChar(tableName.Parent.FullName);

				int p = 0;
				foreach (var row in table) {
					var seqType = row.GetValue(3);
					if (!seqType.IsEqualTo(OneValue)) {
						var obName = row.GetValue(2);
						if (obName.IsEqualTo(name)) {
							var obSchema = row.GetValue(1);
							if (obSchema.IsEqualTo(schema)) {
								// Match so return this
								return p;
							}
						}
						++p;
					}
				}
			}

			return -1;
		}

		public ObjectName GetTableName(int offset) {
			var seqInfo = SequenceManager.SequenceInfoTableName;
			if (transaction.RealTableExists(seqInfo)) {
				var table = transaction.GetTable(seqInfo);
				int p = 0;
				foreach (var row in table) {
					var seqType = row.GetValue(3);
					if (!seqType.IsEqualTo(OneValue)) {
						if (offset == p) {
							var obSchema = row.GetValue(1);
							var obName = row.GetValue(2);
							return new ObjectName(ObjectName.Parse(obSchema.Value.ToString()), obName.Value.ToString());
						}
						++p;
					}
				}
			}

			throw new ArgumentOutOfRangeException("offset");
		}

		public TableInfo GetTableInfo(int offset) {
			var tableName = GetTableName(offset);
			return CreateTableInfo(tableName.Parent, tableName.Name);
		}

		public string GetTableType(int offset) {
			return TableTypes.Sequence;
		}

		public bool ContainsTable(ObjectName name) {
			var seqInfo = SequenceManager.SequenceInfoTableName;

			// This set can not contain the table that is backing it, so we always
			// return false for that.  This check stops an annoying recursive
			// situation for table name resolution.
			if (name.Equals(seqInfo))
				return false;

			return FindByName(name) != -1;
		}

		public ITable GetTable(int offset) {
			var table = transaction.GetTable(SequenceManager.SequenceInfoTableName);
			int p = 0;
			int rowIndex = -1;

			foreach (var row in table) {
				var seqType = row.GetValue(3);
				if (!seqType.IsEqualTo(OneValue)) {
					if (p == offset) {
						rowIndex = row.RowId.RowNumber;
						break;
					}

					p++;
				}
			}

			if (rowIndex == -1)
				throw new ArgumentOutOfRangeException("offset");

			var seqId = table.GetValue(rowIndex, 0);
			var schema = ObjectName.Parse(table.GetValue(rowIndex, 1).Value.ToString());
			var name = table.GetValue(rowIndex, 2).Value.ToString();

			var tableName = new ObjectName(schema, name);

			// Find this id in the 'sequence' table
			var seqTable = transaction.GetTable(SequenceManager.SequenceTableName);

			var index = seqTable.GetIndex(0);
			var list = index.SelectEqual(seqId);

			if (!list.Any())
				throw new Exception("No SEQUENCE table entry for sequence.");

			int seqRowI = list.First();

			// Generate the DataTableInfo
			var tableInfo = CreateTableInfo(schema, name);

			// Last value for this sequence generated by the transaction
			Field lastValue;
			try {
				var sequence = Manager.GetSequence(tableName);
				if (sequence == null)
					throw new ObjectNotFoundException(tableName);

				lastValue = Field.Number(sequence.GetCurrentValue());
			} catch (Exception) {
				lastValue = Field.BigInt(-1);
			}

			// The current value of the sequence generator
			var currentValue = Field.Number(Manager.GetCurrentValue(tableName));

			// Read the rest of the values from the SEQUENCE table.
			var topValue = seqTable.GetValue(seqRowI, 1);
			var incrementBy = seqTable.GetValue(seqRowI, 2);
			var minValue = seqTable.GetValue(seqRowI, 3);
			var maxValue = seqTable.GetValue(seqRowI, 4);
			var start = seqTable.GetValue(seqRowI, 5);
			var cache = seqTable.GetValue(seqRowI, 6);
			var cycle = seqTable.GetValue(seqRowI, 7);


			return new SequenceTable(transaction.Context, tableInfo) {
				TopValue = topValue,
				LastValue = lastValue,
				CurrentValue = currentValue,
				Increment = incrementBy,
				MinValue = minValue,
				MaxValue = maxValue,
				Start = start,
				Cache = cache,
				Cycle = cycle
			};
		}

		#region SequenceTable

		class SequenceTable : GeneratedTable {
			private readonly TableInfo tableInfo;

			public SequenceTable(IContext dbContext, TableInfo tableInfo)
				: base(dbContext) {
				this.tableInfo = tableInfo;
			}

			public override TableInfo TableInfo {
				get { return tableInfo; }
			}

			public override int RowCount {
				get { return 1; }
			}

			public Field TopValue { get; set; }

			public Field LastValue { get; set; }

			public Field CurrentValue { get; set; }

			public Field Increment { get; set; }

			public Field MinValue { get; set; }

			public Field MaxValue { get; set; }

			public Field Start { get; set; }

			public Field Cache { get; set; }

			public Field Cycle { get; set; }

			public override Field GetValue(long rowNumber, int columnOffset) {
				if (rowNumber != 0)
					throw new ArgumentOutOfRangeException("rowNumber");

				switch (columnOffset) {
					case 0:
						return LastValue;
					case 1:
						return CurrentValue;
					case 2:
						return TopValue;
					case 3:
						return Increment;
					case 4:
						return MinValue;
					case 5:
						return MaxValue;
					case 6:
						return Start;
					case 7:
						return Cache;
					case 8:
						return Cycle;
					default:
						throw new ArgumentOutOfRangeException("columnOffset");
				}
			}
		}

		#endregion
	}
}
