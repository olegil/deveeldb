// 
//  Copyright 2010  Deveel
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

using System;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of TType for date objects.
	/// </summary>
	[Serializable]
	public class TDateType : TType {
		public TDateType(SqlType sql_type)
			: base(sql_type) {
		}

		/// <inheritdoc/>
		public override bool IsComparableType(TType type) {
			return (type is TDateType);
		}

		/// <inheritdoc/>
		public override int Compare(Object ob1, Object ob2) {
			return ((DateTime)ob1).CompareTo((DateTime)ob2);
		}

		/// <inheritdoc/>
		public override int CalculateApproximateMemoryUse(Object ob) {
			return 4 + 8;
		}

		/// <inheritdoc/>
		public override Type GetObjectType() {
			return typeof(DateTime);
		}
	}
}