// Copyright (C) 2011, Krzysztof Kozmic 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the <organization> nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Norman
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Mono.Cecil;

	public class TypeNorm : INorm
	{
		private readonly AssemblyNorm assembly;
		private readonly List<Predicate<TypeDefinition>[]> norms = new List<Predicate<TypeDefinition>[]>();
		private readonly Predicate<TypeDefinition> typeDiscovery;

		public TypeNorm(AssemblyNorm assembly, Predicate<TypeDefinition> typeDiscovery)
		{
			this.assembly = assembly;
			this.typeDiscovery = typeDiscovery;
		}

		public TypeNorm Is(params Predicate<TypeDefinition>[] norms)
		{
			this.norms.Add(norms);
			return this;
		}

		private string BuildFailureMessage(HashSet<TypeDefinition> failedTypes)
		{
			var message = new StringBuilder();
			message.AppendFormat("The following {0} types don't conform to the norm.", failedTypes.Count);
			message.AppendLine();
			foreach (var type in failedTypes)
			{
				message.AppendLine(type.FullName);
			}
			return message.ToString();
		}

		private IEnumerable<TypeDefinition> GetMatchedTypes()
		{
			foreach (var assemblyDefinition in assembly.GetMatchedAssemblies())
			{
				foreach (var module in assemblyDefinition.Modules)
				{
					foreach (var type in module.Types)
					{
						if (typeDiscovery(type))
						{
							yield return type;
						}
					}
				}
			}
		}

		void INorm.Verify(IAssert assert)
		{
			var unmatchedTypes = new HashSet<TypeDefinition>();
			var matchedTypes = GetMatchedTypes();
			foreach (var type in matchedTypes)
			{
				if (norms.Exists(n => n.All(x => x(type))) == false)
				{
					unmatchedTypes.Add(type);
				}
			}

			assert.IsTrue(unmatchedTypes, t => t.Count == 0, BuildFailureMessage);
		}
	}
}