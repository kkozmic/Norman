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

namespace Norman.Tests
{
	using System;
	using System.Linq;

	using Norman.Tests.Types;
	using Norman.Tests.Types.ViewModels;

	using Xunit;
	using Xunit.Sdk;

	public class SimpleNormForTypesTests
	{
		private readonly Norm norm;

		public SimpleNormForTypesTests()
		{
			norm = new Norm(new SimpleAssert(message => new AssertException(message)));
		}

		[Fact]
		public void Can_detect_types_in_assembly()
		{
			norm.ForAssemblies(a => a.FullName.Contains(".Tests"))
				.ForTypes(t => t.Name.EndsWith("Tests")).Is(n => n.IsPublic);

			norm.Verify();
		}

		[Fact]
		public void Can_detect_types_not_implementing_interface()
		{
			norm.ForAssemblies(a => a.FullName.Contains(".Tests"))
				.ForTypes(t => t.Properties.Any(p => p.Name == "IsActive"))
				.MustImplement<IActivable>();

			var exception = Assert.Throws<AssertException>(() => norm.Verify());
			var message = string.Format("The following 1 types don't conform to the norm.{0}{1}{0}", Environment.NewLine,
			                            typeof(HasIsActiveProperty).FullName);
			Assert.Equal(message, exception.Message);
		}

		[Fact]
		public void Can_detect_types_referencing_forbidden_types()
		{
			norm.ForAssemblies(a => a.FullName.Contains(".Tests"))
				.ForTypes(t => t.Namespace.EndsWith(".ViewModels"))
				.MustNotCallAny(t => t.Namespace.EndsWith(".Services"));

			var exception = Assert.Throws<AssertException>(() => norm.Verify());
			var message = string.Format("The following 1 types don't conform to the norm.{0}{1}{0}", Environment.NewLine,
			                            typeof(BarViewModel).FullName);
			Assert.Equal(message, exception.Message);
		}

		[Fact]
		public void Can_detect_types_using_specified_method()
		{
			norm.ForAssemblies(a => a.FullName.Contains(".Tests"))
				.ForTypes(t => t.Namespace.EndsWith(".Types"))
				.MustNotCall<object, DateTime>(o => DateTime.Now);

			var exception = Assert.Throws<AssertException>(() => norm.Verify());
			var message = string.Format("The following 1 types don't conform to the norm.{0}{1}{0}", Environment.NewLine,
			                            typeof(UseDateTimeNow).FullName);

			Assert.Equal(message, exception.Message);
		}
	}
}