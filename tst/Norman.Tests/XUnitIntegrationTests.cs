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

	using Norman.XUnitIntegration;

	using Xunit;
	using Xunit.Sdk;

	public class XUnitIntegrationTests
	{
		private readonly XUnitDiscovery xunitDiscovery;

		public XUnitIntegrationTests()
		{
			xunitDiscovery = new XUnitDiscovery();
		}

		[Fact]
		public void Can_create_assert()
		{
			Assert.NotNull(xunitDiscovery.BuildAssert(typeof(FactAttribute).Assembly));
		}

		[Fact]
		public void Can_detect_xunit()
		{
			Assert.NotNull(xunitDiscovery.Detect(AppDomain.CurrentDomain.GetAssemblies()));
		}

		[Fact]
		public void Can_execute_assert_and_fail()
		{
			var assert = xunitDiscovery.BuildAssert(typeof(FactAttribute).Assembly);

			var item = new object();
			var exception = Assert.Throws<AssertException>(() => assert.IsTrue(item, o => false, o => "custom message"));

			Assert.Equal("custom message", exception.Message);
			Assert.Same(item, exception.Data["norman.object"]);
		}

		[Fact]
		public void Can_execute_assert_and_pass()
		{
			var assert = xunitDiscovery.BuildAssert(typeof(FactAttribute).Assembly);

			Assert.DoesNotThrow(() => assert.IsTrue(new object(), o => true, o => null));
		}
	}
}