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

	using Mono.Cecil;

	using Norman.TestFrameworkIntegration;

	public class Norm
	{
		private static readonly IAssertBuilder defaultAssertBuilder = new SimpleAssertBuilder(new ITestFrameworkDiscovery[]
		{
			new NUnitDiscovery(),
			new XUnitDiscovery(),
			new MsTestDiscovery(),
		});

		private readonly IAssertBuilder assertBuilder;

		private readonly List<INorm> inner = new List<INorm>();

		private IAssert assert;

		public Norm(IAssertBuilder assertBuilder)
		{
			this.assertBuilder = assertBuilder;
		}

		public Norm(IAssert assert)
		{
			this.assert = assert;
		}

		public AssemblyNorm ForAssemblies(Predicate<AssemblyDefinition> assemblyDiscovery)
		{
			var norm = new AssemblyNorm(assemblyDiscovery);
			inner.Add(norm);
			return norm;
		}

		public TypeNorm ForTypes(Predicate<TypeDefinition> typeDiscovery)
		{
			return ForAssemblies(null).ForTypes(typeDiscovery);
		}

		public void Verify()
		{
			if (assert == null)
			{
				assert = assertBuilder.CreateAssert();
			}
			foreach (var norm in inner)
			{
				norm.Verify(assert);
			}
		}

		public static Norm Build()
		{
			return new Norm(defaultAssertBuilder);
		}
	}
}