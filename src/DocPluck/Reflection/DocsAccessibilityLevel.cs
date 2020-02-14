// DocPluck
// Copyright 2020 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DocPluck.Reflection
{
	/// <summary>
	/// Contains the accessibility levels of members.
	/// </summary>
	public enum DocsAccessibilityLevel
	{
		/// <summary>
		/// The public accessibility level.
		/// </summary>
		Public,

		/// <summary>
		/// The protected accessibility level.
		/// </summary>
		Protected,

		/// <summary>
		/// The internal accessibility level.
		/// </summary>
		Internal,

		/// <summary>
		/// The protected internal accessibility level.
		/// </summary>
		ProtectedInternal,

		/// <summary>
		/// The private accessibility level.
		/// </summary>
		Private,

		/// <summary>
		/// The private protected accessibility level.
		/// </summary>
		PrivateProtected,

		/// <summary>
		/// An unknown accessibility level.
		/// </summary>
		Unknown
	}
}