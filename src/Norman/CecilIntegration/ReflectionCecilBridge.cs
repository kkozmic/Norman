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

namespace Norman.CecilIntegration
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Mono.Cecil;

	public static class ReflectionCecilBridge
	{
		public static AssemblyDefinition ResolveAssembly(this Assembly assembly)
		{
			// NOTE: this could use some caching I suppose and loading via stream is probably a better option too
			return AssemblyDefinition.ReadAssembly(assembly.Location);
		}

		public static MethodDefinition ResolveMethod(this MethodInfo method)
		{
			return ResolveType(method.DeclaringType).Methods.Single(m => AreSameMethod(method, m));
		}

		public static TypeDefinition ResolveType(this Type type)
		{
			return type.Assembly.ResolveAssembly().Modules.Select(m => m.GetType(type.FullName)).Single();
		}

		private static bool AreSameMethod(MethodInfo method, MethodReference methodReference)
		{
			var calledMethod = methodReference.Resolve();
			var token = calledMethod.MetadataToken.ToInt32();
			return token == method.MetadataToken;
		}
	}
}