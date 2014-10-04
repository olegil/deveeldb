﻿// 
//  Copyright 2010-2014 Deveel
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

using Deveel.Data.Routines;

namespace Deveel.Data.Client {
	public sealed class TriggerInvoke {
		public TriggerInvoke(string triggerName, string objectName, TriggerEventType eventType, int count) {
			Count = count;
			EventType = eventType;
			ObjectName = objectName;
			TriggerName = triggerName;
		}

		public string TriggerName { get; private set; }

		public string ObjectName { get; private set; }

		public TriggerEventType EventType { get; private set; }

		public int Count { get; private set; }

		public bool IsBefore {
			get { return (EventType & TriggerEventType.Before) != 0; }
		}

		public bool IsAfter {
			get { return (EventType & TriggerEventType.After) != 0; }
		}

		public bool IsInsert {
			get { return (EventType & TriggerEventType.Insert) != 0; }
		}

		public bool IsUpdate {
			get { return (EventType & TriggerEventType.Update) != 0; }
		}

		public bool IsDelete {
			get { return (EventType & TriggerEventType.Delete) != 0; }
		}
	}

}