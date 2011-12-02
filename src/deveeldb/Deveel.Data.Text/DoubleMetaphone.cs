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
using System.Text;

namespace Deveel.Data.Text {
	public sealed class DoubleMetaphone {
		#region ctor
		public DoubleMetaphone() {
		}
		#endregion

		#region Fields
		private int maxCodeLen = 4;

		private static readonly string VOWELS = "AEIOUY";

		private static readonly string[] SILENT_START = { "GN", "KN", "PN", "WR", "PS" };
		private static readonly string[] L_R_N_M_B_H_F_V_W_SPACE = { "L", "R", "N", "M", "B", "H", "F", "V", "W", " " };
		private static readonly String[] ES_EP_EB_EL_EY_IB_IL_IN_IE_EI_ER = { "ES", "EP", "EB", "EL", "EY", "IB", "IL", "IN", "IE", "EI", "ER" };
		private static readonly string[] L_T_K_S_N_M_B_Z = { "L", "T", "K", "S", "N", "M", "B", "Z" };
		#endregion

		#region Properties
		public int MaxCodeLength {
			get { return maxCodeLen; }
			set { maxCodeLen = value; }
		}
		#endregion

		#region Private Methods
		private char GetChar(string value, int index) {
			if (index < 0 || index >= value.Length)
				return Char.MinValue;

			return value[index];
		}

		private int HandleAEIOUY(string value, DoubleMetaphoneResult result, int index) {
			if (index == 0) {
				result.Append('A');
			}
			return index + 1;
		}

		private int HandleC(string value, DoubleMetaphoneResult result,  int index) {
			if (ConditionC0(value, index)) {  // very confusing, moved out
				result.Append('K');
				index += 2;
			} else if (index == 0 && Contains(value, index, 6, "CAESAR")) {
				result.Append('S');
				index += 2;
			} else if (Contains(value, index, 2, "CH")) {
				index = HandleCH(value, result, index);
			} else if (Contains(value, index, 2, "CZ") && 
				!Contains(value, index - 2, 4, "WICZ")) {
				//-- "Czerny" --//
				result.Append('S', 'X');
				index += 2;
			} else if (Contains(value, index + 1, 3, "CIA")) {
				//-- "focaccia" --//
				result.Append('X');
				index += 3;
			} else if (Contains(value, index, 2, "CC") && 
				!(index == 1 && GetChar(value, 0) == 'M')) {
				//-- double "cc" but not "McClelland" --//
				return HandleCC(value, result, index);
			} else if (Contains(value, index, 2, "CK", "CG", "CQ")) {
				result.Append('K');
				index += 2;
			} else if (Contains(value, index, 2, "CI", "CE", "CY")) {
				//-- Italian vs. English --//
				if (Contains(value, index, 3, "CIO", "CIE", "CIA")) {
					result.Append('S', 'X');
				} else {
					result.Append('S');
				}
				index += 2;
			} else {
				result.Append('K');
				if (Contains(value, index + 1, 2, " C", " Q", " G")) { 
					//-- Mac Caffrey, Mac Gregor --//
					index += 3;
				} else if (Contains(value, index + 1, 1, "C", "K", "Q") && 
					!Contains(value, index + 1, 2, "CE", "CI")) {
					index += 2;
				} else {
					index++;
				}
			}

			return index;
		}

		private int HandleCC(string value, DoubleMetaphoneResult result, int index) {
			if (Contains(value, index + 2, 1, "I", "E", "H") && 
				!Contains(value, index + 2, 2, "HU")) {
				//-- "bellocchio" but not "bacchus" --//
				if ((index == 1 && GetChar(value, index - 1) == 'A') || 
					Contains(value, index - 1, 5, "UCCEE", "UCCES")) {
					//-- "accident", "accede", "succeed" --//
					result.Append("KS");
				} else {
					//-- "bacci", "bertucci", other Italian --//
					result.Append('X');
				}
				index += 3;
			} else {    // Pierce's rule
				result.Append('K');
				index += 2;
			}
        
			return index;
		}

		private int HandleCH(string value, DoubleMetaphoneResult result, int index) {
			if (index > 0 && Contains(value, index, 4, "CHAE")) {   // Michael
				result.Append('K', 'X');
				return index + 2;
			} else if (ConditionCH0(value, index)) {
				//-- Greek roots ("chemistry", "chorus", etc.) --//
				result.Append('K');
				return index + 2;
			} else if (ConditionCH1(value, index)) {
				//-- Germanic, Greek, or otherwise 'ch' for 'kh' sound --//
				result.Append('K');
				return index + 2;
			} else {
				if (index > 0) {
					if (Contains(value, 0, 2, "MC")) {
						result.Append('K');
					} else {
						result.Append('X', 'K');
					}
				} else {
					result.Append('X');
				}
				return index + 2;
			}
		}

		private int HandleD(string value, DoubleMetaphoneResult result, int index) {
			if (Contains(value, index, 2, "DG")) {
				//-- "Edge" --//
				if (Contains(value, index + 2, 1, "I", "E", "Y")) {
					result.Append('J');
					index += 3;
					//-- "Edgar" --//
				} else {
					result.Append("TK");
					index += 2;
				}
			} else if (Contains(value, index, 2, "DT", "DD")) {
				result.Append('T');
				index += 2;
			} else {
				result.Append('T');
				index++;
			}
			return index;
		}

		private int HandleG(string value, DoubleMetaphoneResult result, int index, bool slavoGermanic) {
			if (GetChar(value, index + 1) == 'H') {
				index = HandleGH(value, result, index);
			} else if (GetChar(value, index + 1) == 'N') {
				if (index == 1 && IsVowel(GetChar(value, 0)) && !slavoGermanic) {
					result.Append("KN", "N");
				} else if (!Contains(value, index + 2, 2, "EY") && 
					GetChar(value, index + 1) != 'Y' && !slavoGermanic) {
					result.Append("N", "KN");
				} else {
					result.Append("KN");
				}
				index = index + 2;
			} else if (Contains(value, index + 1, 2, "LI") && !slavoGermanic) {
				result.Append("KL", "L");
				index += 2;
			} else if (index == 0 && (GetChar(value, index + 1) == 'Y' || Contains(value, index + 1, 2, ES_EP_EB_EL_EY_IB_IL_IN_IE_EI_ER))) {
				//-- -ges-, -gep-, -gel-, -gie- at beginning --//
				result.Append('K', 'J');
				index += 2;
			} else if ((Contains(value, index + 1, 2, "ER") || 
				GetChar(value, index + 1) == 'Y') &&
				!Contains(value, 0, 6, "DANGER", "RANGER", "MANGER") &&
				!Contains(value, index - 1, 1, "E", "I") && 
				!Contains(value, index - 1, 3, "RGY", "OGY")) {
				//-- -ger-, -gy- --//
				result.Append('K', 'J');
				index += 2;
			} else if (Contains(value, index + 1, 1, "E", "I", "Y") || 
				Contains(value, index - 1, 4, "AGGI", "OGGI")) {
				//-- Italian "biaggi" --//
				if ((Contains(value, 0 ,4, "VAN ", "VON ") || Contains(value, 0, 3, "SCH")) || Contains(value, index + 1, 2, "ET")) {
					//-- obvious germanic --//
					result.Append('K');
				} else if (Contains(value, index + 1, 4, "IER")) {
					result.Append('J');
				} else {
					result.Append('J', 'K');
				}
				index += 2;
			} else if (GetChar(value, index + 1) == 'G') {
				index += 2;
				result.Append('K');
			} else {
				index++;
				result.Append('K');
			}
			return index;
		}

		private int HandleGH(String value, DoubleMetaphoneResult result, int index) {
			if (index > 0 && !IsVowel(GetChar(value, index - 1))) {
				result.Append('K');
				index += 2;
			} else if (index == 0) {
				if (GetChar(value, index + 2) == 'I') {
					result.Append('J');
				} else {
					result.Append('K');
				}
				index += 2;
			} else if ((index > 1 && Contains(value, index - 2, 1, "B", "H", "D")) ||
				(index > 2 && Contains(value, index - 3, 1, "B", "H", "D")) ||
				(index > 3 && Contains(value, index - 4, 1, "B", "H"))) {
				//-- Parker's rule (with some further refinements) - "hugh"
				index += 2;
			} else {
				if (index > 2 && GetChar(value, index - 1) == 'U' && 
					Contains(value, index - 3, 1, "C", "G", "L", "R", "T")) {
					//-- "laugh", "McLaughlin", "cough", "gough", "rough", "tough"
					result.Append('F');
				} else if (index > 0 && GetChar(value, index - 1) != 'I') {
					result.Append('K');
				}
				index += 2;
			}
			return index;
		}

		private int HandleH(string value, DoubleMetaphoneResult result, int index) {
			//-- only keep if first & before vowel or between 2 vowels --//
			if ((index == 0 || IsVowel(GetChar(value, index - 1))) && 
				IsVowel(GetChar(value, index + 1))) {
				result.Append('H');
				index += 2;
				//-- also takes car of "HH" --//
			} else {
				index++;
			}
			return index;
		}

		private int HandleJ(string value, DoubleMetaphoneResult result, int index, bool slavoGermanic) {
			if (Contains(value, index, 4, "JOSE") || Contains(value, 0, 4, "SAN ")) {
				//-- obvious Spanish, "Jose", "San Jacinto" --//
				if ((index == 0 && (GetChar(value, index + 4) == ' ') || 
					value.Length == 4) || Contains(value, 0, 4, "SAN ")) {
					result.Append('H');
				} else {
					result.Append('J', 'H');
				}
				index++;
			} else {
				if (index == 0 && !Contains(value, index, 4, "JOSE")) {
					result.Append('J', 'A');
				} else if (IsVowel(GetChar(value, index - 1)) && !slavoGermanic && 
					(GetChar(value, index + 1) == 'A' || GetChar(value, index + 1) == 'O')) {
					result.Append('J', 'H');
				} else if (index == value.Length - 1) {
					result.Append('J', ' ');
				} else if (!Contains(value, index + 1, 1, L_T_K_S_N_M_B_Z) && 
					!Contains(value, index - 1, 1, "S", "K", "L")) {
					result.Append('J');
				}

				if (GetChar(value, index + 1) == 'J') {
					index += 2;
				} else {
					index++;
				}
			}
			return index;
		}

		private int HandleL(string value, DoubleMetaphoneResult result, int index) {
			result.Append('L');
			if (GetChar(value, index + 1) == 'L') {
				if (ConditionL0(value, index)) {
					result.AppendAlternate(' ');
				}
				index += 2;
			} else {
				index++;
			}
			return index;
		}

		private int HandleP(string value, DoubleMetaphoneResult result, int index) {
			if (GetChar(value, index + 1) == 'H') {
				result.Append('F');
				index += 2;
			} else {
				result.Append('P');
				index = Contains(value, index + 1, 1, "P", "B") ? index + 2 : index + 1;
			}
			return index;
		}

		private int HandleR(string value, DoubleMetaphoneResult result, int index, bool slavoGermanic) {
			if (index == value.Length - 1 && !slavoGermanic && 
				Contains(value, index - 2, 2, "IE") && 
				!Contains(value, index - 4, 2, "ME", "MA")) {
				result.AppendAlternate('R');
			} else {
				result.Append('R');
			}
			return GetChar(value, index + 1) == 'R' ? index + 2 : index + 1;
		}

		private int HandleS(string value, DoubleMetaphoneResult result, int index, bool slavoGermanic) {
			if (Contains(value, index - 1, 3, "ISL", "YSL")) {
				//-- special cases "island", "isle", "carlisle", "carlysle" --//
				index++;
			} else if (index == 0 && Contains(value, index, 5, "SUGAR")) {
				//-- special case "sugar-" --//
				result.Append('X', 'S');
				index++;
			} else if (Contains(value, index, 2, "SH")) {
				if (Contains(value, index + 1, 4, 
					"HEIM", "HOEK", "HOLM", "HOLZ")) {
					//-- germanic --//
					result.Append('S');
				} else {
					result.Append('X');
				}
				index += 2;
			} else if (Contains(value, index, 3, "SIO", "SIA") || Contains(value, index, 4, "SIAN")) {
				//-- Italian and Armenian --//
				if (slavoGermanic) {
					result.Append('S');
				} else {
					result.Append('S', 'X');
				}
				index += 3;
			} else if ((index == 0 && Contains(value, index + 1, 1, "M", "N", "L", "W")) || Contains(value, index + 1, 1, "Z")) {
				//-- german & anglicisations, e.g. "smith" match "schmidt" //
				// "snider" match "schneider" --//
				//-- also, -sz- in slavic language altho in hungarian it //
				//   is pronounced "s" --//
				result.Append('S', 'X');
				index = Contains(value, index + 1, 1, "Z") ? index + 2 : index + 1;
			} else if (Contains(value, index, 2, "SC")) {
				index = HandleSC(value, result, index);
			} else {
				if (index == value.Length - 1 && Contains(value, index - 2, 
					2, "AI", "OI")){
					//-- french e.g. "resnais", "artois" --//
					result.AppendAlternate('S');
				} else {
					result.Append('S');
				}
				index = Contains(value, index + 1, 1, "S", "Z") ? index + 2 : index + 1;
			}
			return index;
		}

		private int HandleSC(string value, DoubleMetaphoneResult result, int index) {
			if (GetChar(value, index + 2) == 'H') {
				//-- Schlesinger's rule --//
				if (Contains(value, index + 3, 2, "OO", "ER", "EN", "UY", "ED", "EM")) {
					//-- Dutch origin, e.g. "school", "schooner" --//
					if (Contains(value, index + 3, 2, "ER", "EN")) {
						//-- "schermerhorn", "schenker" --//
						result.Append("X", "SK");
					} else {
						result.Append("SK");
					}
				} else {
					if (index == 0 && !IsVowel(GetChar(value, 3)) && GetChar(value, 3) != 'W') {
						result.Append('X', 'S');
					} else {
						result.Append('X');
					}
				}
			} else if (Contains(value, index + 2, 1, "I", "E", "Y")) {
				result.Append('S');
			} else {
				result.Append("SK");
			}
			return index + 3;
		}

		private int HandleT(string value, DoubleMetaphoneResult result, int index) {
			if (Contains(value, index, 4, "TION")) {
				result.Append('X');
				index += 3;
			} else if (Contains(value, index, 3, "TIA", "TCH")) {
				result.Append('X');
				index += 3;
			} else if (Contains(value, index, 2, "TH") || Contains(value, index, 
				3, "TTH")) {
				if (Contains(value, index + 2, 2, "OM", "AM") || 
					//-- special case "thomas", "thames" or germanic --//
					Contains(value, 0, 4, "VAN ", "VON ") || 
					Contains(value, 0, 3, "SCH")) {
					result.Append('T');
				} else {
					result.Append('0', 'T');
				}
				index += 2;
			} else {
				result.Append('T');
				index = Contains(value, index + 1, 1, "T", "D") ? index + 2 : index + 1;
			}
			return index;
		}

		private int HandleW(string value, DoubleMetaphoneResult result, int index) {
			if (Contains(value, index, 2, "WR")) {
				//-- can also be in middle of word --//
				result.Append('R');
				index += 2;
			} else {
				if (index == 0 && (IsVowel(GetChar(value, index + 1)) || 
					Contains(value, index, 2, "WH"))) {
					if (IsVowel(GetChar(value, index + 1))) {
						//-- Wasserman should match Vasserman --//
						result.Append('A', 'F');
					} else {
						//-- need Uomo to match Womo --//
						result.Append('A');
					}
					index++;
				} else if ((index == value.Length - 1 && IsVowel(GetChar(value, index - 1))) ||
					Contains(value, index - 1, 5, "EWSKI", "EWSKY", "OWSKI", "OWSKY") ||
					Contains(value, 0, 3, "SCH")) {
					//-- Arnow should match Arnoff --//
					result.AppendAlternate('F');
					index++;
				} else if (Contains(value, index, 4, "WICZ", "WITZ")) {
					//-- Polish e.g. "filipowicz" --//
					result.Append("TS", "FX");
					index += 4;
				} else {
					index++;
				}
			}
			return index;
		}

		private int HandleX(string value, DoubleMetaphoneResult result, int index) {
			if (index == 0) {
				result.Append('S');
				index++;
			} else {
				if (!((index == value.Length - 1) && 
					(Contains(value, index - 3, 3, "IAU", "EAU") || 
					Contains(value, index - 2, 2, "AU", "OU")))) {
					//-- French e.g. breaux --//
					result.Append("KS");
				}
				index = Contains(value, index + 1, 1, "C", "X") ? index + 2 : index + 1;
			}
			return index;
		}

		private int HandleZ(string value, DoubleMetaphoneResult result, int index, bool slavoGermanic) {
			if (GetChar(value, index + 1) == 'H') {
				//-- Chinese pinyin e.g. "zhao" or Angelina "Zhang" --//
				result.Append('J');
				index += 2;
			} else {
				if (Contains(value, index + 1, 2, "ZO", "ZI", "ZA") || (slavoGermanic && (index > 0 && GetChar(value, index - 1) != 'T'))) {
					result.Append("S", "TS");
				} else {
					result.Append('S');
				}
				index = GetChar(value, index + 1) == 'Z' ? index + 2 : index + 1;
			}
			return index;
		}


		private bool ConditionC0(string value, int index) {
			if (Contains(value, index, 4, "CHIA")) {
				return true;
			} else if (index <= 1) {
				return false;
			} else if (IsVowel(GetChar(value, index - 2))) {
				return false;
			} else if (!Contains(value, index - 1, 3, "ACH")) {
				return false;
			} else {
				char c = GetChar(value, index + 2);
				return (c != 'I' && c != 'E')
					|| Contains(value, index - 2, 6, "BACHER", "MACHER");
			}
		}

		private bool ConditionCH0(String value, int index) {
			if (index != 0) {
				return false;
			} else if (!Contains(value, index + 1, 5, "HARAC", "HARIS") && 
				!Contains(value, index + 1, 3, "HOR", "HYM", "HIA", "HEM")) {
				return false;
			} else if (Contains(value, 0, 5, "CHORE")) {
				return false;
			} else {
				return true;
			}
		}

		private bool ConditionCH1(string value, int index) {
			return ((Contains(value, 0, 4, "VAN ", "VON ") || Contains(value, 0, 3, "SCH")) ||
				Contains(value, index - 2, 6, "ORCHES", "ARCHIT", "ORCHID") ||
				Contains(value, index + 2, 1, "T", "S") ||
				((Contains(value, index - 1, 1, "A", "O", "U", "E") || index == 0) &&
				(Contains(value, index + 2, 1, L_R_N_M_B_H_F_V_W_SPACE) || index + 1 == value.Length - 1)));
		}

		private bool ConditionL0(String value, int index) {
			if (index == value.Length - 3 && 
				Contains(value, index - 1, 4, "ILLO", "ILLA", "ALLE")) {
				return true;
			} else if ((Contains(value, index - 1, 2, "AS", "OS") || 
				Contains(value, value.Length - 1, 1, "A", "O")) &&
				Contains(value, index - 1, 4, "ALLE")) {
				return true;
			} else {
				return false;
			}
		}

		private bool ConditionM0(String value, int index) {
			if (GetChar(value, index + 1) == 'M') {
				return true;
			}
			return Contains(value, index - 1, 3, "UMB")
				&& ((index + 1) == value.Length - 1 || Contains(value, index + 2, 2, "ER"));
		}


		private bool IsSlavoGermanic(string value) {
			return value.IndexOf('W') > -1 || value.IndexOf('K') > -1 || 
				value.IndexOf("CZ") > -1 || value.IndexOf("WITZ") > -1;
		}

		private bool IsVowel(char ch) {
			return VOWELS.IndexOf(ch) != -1;
		}

		private bool IsSilentStart(String value) {
			bool result = false;
			for (int i = 0; i < SILENT_START.Length; i++) {
				if (value.StartsWith(SILENT_START[i])) {
					result = true;
					break;
				}
			}
			return result;
		}

		private string cleanInput(String input) {
			if (input == null) {
				return null;
			}
			input = input.Trim();
			if (input.Length == 0) {
				return null;
			}
			return input.ToUpper();
		}
		#endregion

		#region Private Static Methods
		private static bool Contains(string value, int start, int length, params string[] criteria) {
			bool result = false;
			if (start >= 0 && start + length <= value.Length) {
				string target = value.Substring(start, start + length);

				for (int i = 0; i < criteria.Length; i++) {
					if (target.Equals(criteria[i])) {
						result = true;
						break;
					}
				}
			}
			return result;
		}
		#endregion

		#region Public Methods
		public string Compute(string value) {
			return Compute(value, false);
		}

		public string Compute(string value, bool alternate) {
			value = cleanInput(value);
			if (value == null) {
				return null;
			}
        
			bool slavoGermanic = IsSlavoGermanic(value);
			int index = IsSilentStart(value) ? 1 : 0;

			DoubleMetaphoneResult result = new DoubleMetaphoneResult(this.maxCodeLen);
        
			while (!result.IsComplete && index <= value.Length - 1) {
				switch (value[index]) {
					case 'A':
					case 'E':
					case 'I':
					case 'O':
					case 'U':
					case 'Y':
						index = HandleAEIOUY(value, result, index);
						break;
					case 'B':
						result.Append('P');
						index = GetChar(value, index + 1) == 'B' ? index + 2 : index + 1;
						break;
					case '\u00C7':
						// A C with a Cedilla
						result.Append('S');
						index++;
						break; 
					case 'C':
						index = HandleC(value, result, index);
						break;
					case 'D':
						index = HandleD(value, result, index);
						break;
					case 'F':
						result.Append('F');
						index = GetChar(value, index + 1) == 'F' ? index + 2 : index + 1;
						break;
					case 'G':
						index = HandleG(value, result, index, slavoGermanic);
						break;
					case 'H':
						index = HandleH(value, result, index);
						break;
					case 'J':
						index = HandleJ(value, result, index, slavoGermanic);
						break;
					case 'K':
						result.Append('K');
						index = GetChar(value, index + 1) == 'K' ? index + 2 : index + 1;
						break;
					case 'L':
						index = HandleL(value, result, index);
						break;
					case 'M':
						result.Append('M');
						index = ConditionM0(value, index) ? index + 2 : index + 1;
						break;
					case 'N':
						result.Append('N');
						index = GetChar(value, index + 1) == 'N' ? index + 2 : index + 1;
						break;
					case '\u00D1':
						// N with a tilde (spanish ene)
						result.Append('N');
						index++;
						break;
					case 'P':
						index = HandleP(value, result, index);
						break;
					case 'Q':
						result.Append('K');
						index = GetChar(value, index + 1) == 'Q' ? index + 2 : index + 1;
						break;
					case 'R':
						index = HandleR(value, result, index, slavoGermanic);
						break;
					case 'S':
						index = HandleS(value, result, index, slavoGermanic);
						break;
					case 'T':
						index = HandleT(value, result, index);
						break;
					case 'V':
						result.Append('F');
						index = GetChar(value, index + 1) == 'V' ? index + 2 : index + 1;
						break;
					case 'W':
						index = HandleW(value, result, index);
						break;
					case 'X':
						index = HandleX(value, result, index);
						break;
					case 'Z':
						index = HandleZ(value, result, index, slavoGermanic);
						break;
					default:
						index++;
						break;
				}
			}

			return alternate ? result.Alternate : result.Primary;
		}
		#endregion


		#region DoubleMetaphoneResult
		class DoubleMetaphoneResult {
			#region ctor
			public DoubleMetaphoneResult(int maxCodeLen) {
				this.maxLength = maxCodeLen;
				primary = new StringBuilder(maxCodeLen);
				alternate = new StringBuilder(maxCodeLen);
			}
			#endregion

			#region Fields
			private StringBuilder primary;
			private StringBuilder alternate;
			private int maxLength;
			#endregion

			#region Properties
			public string Primary {
				get { return this.primary.ToString(); }
			}

			public string Alternate {
				get { return this.alternate.ToString(); }
			}

			public bool IsComplete {
				get {
					return this.primary.Length >= this.maxLength &&
						this.alternate.Length >= this.maxLength;
				}
			}
			#endregion

			#region Public Methods
			public void Append(char value) {
				AppendPrimary(value);
				AppendAlternate(value);
			}

			public void Append(char primary, char alternate) {
				AppendPrimary(primary);
				AppendAlternate(alternate);
			}

			public void AppendPrimary(char value) {
				if (this.primary.Length < this.maxLength) {
					this.primary.Append(value);
				}
			}

			public void AppendAlternate(char value) {
				if (this.alternate.Length < this.maxLength) {
					this.alternate.Append(value);
				}
			}

			public void Append(string value) {
				AppendPrimary(value);
				AppendAlternate(value);
			}

			public void Append(string primary, string alternate) {
				AppendPrimary(primary);
				AppendAlternate(alternate);
			}

			public void AppendPrimary(string value) {
				int addChars = this.maxLength - this.primary.Length;
				if (value.Length <= addChars) {
					this.primary.Append(value);
				} else {
					this.primary.Append(value.Substring(0, addChars));
				}
			}

			public void AppendAlternate(string value) {
				int addChars = this.maxLength - this.alternate.Length;
				if (value.Length <= addChars) {
					this.alternate.Append(value);
				} else {
					this.alternate.Append(value.Substring(0, addChars));
				}
			}
			#endregion
		}
		#endregion
	}
}