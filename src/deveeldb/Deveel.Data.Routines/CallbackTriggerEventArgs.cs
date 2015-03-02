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
//

using System;

namespace Deveel.Data.Routines {
	public delegate void CallbackTriggerEventHandler(object sender, CallbackTriggerEventArgs args);

	public sealed class CallbackTriggerEventArgs : EventArgs {
		internal CallbackTriggerEventArgs(ObjectName triggerName, ObjectName triggerSource, TriggerEventType triggerEventType, int fireCount) {
			TriggerName = triggerName;
			TriggerSource = triggerSource;
			TriggerEventType = triggerEventType;
			FireCount = fireCount;
		}

		public ObjectName TriggerName { get; private set; }

		public ObjectName TriggerSource { get; private set; }

		public TriggerEventType TriggerEventType { get; private set; }

		public int FireCount { get; private set; }
	}
}