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
using System.Globalization;

namespace Deveel.Data.Diagnostics {
	public static class EventExtensions {
		public static T GetData<T>(this IEvent @event, string key) {
			if (@event == null || @event.EventData == null)
				return default(T);

			object value;
			if (!@event.EventData.TryGetValue(key, out value))
				return default(T);

			if (value is T)
				return (T) value;

#if !PCL
			if (value is string &&
			    typeof(T).IsEnum)
				return (T) Enum.Parse(typeof(T), (string) value, true);

			if (value is IConvertible)
				return (T) Convert.ChangeType(value, typeof (T), CultureInfo.InvariantCulture);

			throw new InvalidCastException();
#else
			return (T) Convert.ChangeType(value, typeof (T), CultureInfo.InvariantCulture);
#endif
		}
	}
}
