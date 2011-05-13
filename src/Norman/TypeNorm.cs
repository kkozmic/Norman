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
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Text;

	using Mono.Cecil;
	using Mono.Cecil.Cil;

	using Norman.CecilIntegration;

	public class TypeNorm : INorm
	{
		private readonly AssemblyNorm assembly;
		private readonly List<Predicate<TypeDefinition>> norms = new List<Predicate<TypeDefinition>>();
		private readonly Predicate<TypeDefinition> typeDiscovery;

		public TypeNorm(AssemblyNorm assembly, Predicate<TypeDefinition> typeDiscovery)
		{
			this.assembly = assembly;
			this.typeDiscovery = typeDiscovery;
		}

		public TypeNorm Is(Predicate<TypeDefinition> norm)
		{
			norms.Add(norm);
			return this;
		}

		public TypeNorm MustImplement<TInterface>()
		{
			var @interface = typeof(TInterface).ResolveType();
			norms.Add(t => t.Interfaces.Contains(@interface));
			return this;
		}

		public TypeNorm MustNotCall<TType>(Expression<Action<TType>> callExpression)
		{
			var method = ExtractCalledMethod(callExpression);

			norms.Add(t => IsCalling(t, method) == false);
			return this;
		}

		public TypeNorm MustNotCall<TType, TOut>(Expression<Func<TType, TOut>> callExpression)
		{
			var method = ExtractCalledMethod(callExpression);

			norms.Add(t => IsCalling(t, method) == false);
			return this;
		}

		public TypeNorm MustNotCall<TOut>(Expression<Func<TOut>> callExpression)
		{
			var method = ExtractCalledMethod(callExpression);
			norms.Add(t => IsCalling(t, method) == false);
			return this;
		}

		public void MustNotCallAny(Predicate<TypeDefinition> matchTypes)
		{
			var types = new Lazy<IEnumerable<TypeDefinition>>(() => GetAllTypes().Where(matchTypes.Invoke));
			norms.Add(t => IsCallingAny(t, types.Value) == false);
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

		private MethodDefinition ExtractCalledMethod(LambdaExpression expression)
		{
			var body = (MemberExpression)expression.Body;
			var property = body.Member as PropertyInfo;
			if (property != null)
			{
				return property.GetGetMethod(true).ResolveMethod();
			}
			return ((MethodInfo)body.Member).ResolveMethod();
		}

		private IEnumerable<TypeDefinition> GetAllTypes()
		{
			return assembly.GetMatchedAssemblies()
				.SelectMany(a => a.Modules)
				.SelectMany(m => m.Types);
		}

		private IEnumerable<TypeDefinition> GetMatchedTypes()
		{
			return GetAllTypes().Where(typeDiscovery.Invoke);
		}

		private bool IsCalling(TypeDefinition type, MethodDefinition method)
		{
			return type.Methods.Any(m => IsCalling(m, method));
		}

		private bool IsCalling(MethodDefinition scannedMethod, MethodDefinition method)
		{
			if (scannedMethod.HasBody == false)
			{
				return false;
			}
			foreach (var instruction in scannedMethod.Body.Instructions)
			{
				if (IsMethodCall(instruction) == false)
				{
					continue;
				}
				var methodReference = (MethodReference)instruction.Operand;
				if (methodReference.Name != method.Name)
				{
					continue;
				}
				var methodDefinition = methodReference.Resolve();
				if (method.MetadataToken.ToInt32() == methodDefinition.MetadataToken.ToInt32())
				{
					return true;
				}
			}
			return false;
		}

		private bool IsCalling(MethodDefinition scannedMethod, TypeDefinition typeDefinition)
		{
			return typeDefinition.Methods.Any(m => IsCalling(scannedMethod, m));
		}

		private bool IsCallingAny(TypeDefinition type, IEnumerable<TypeDefinition> types)
		{
			return type.Methods.Any(m => IsCallingAny(m, types));
		}

		private bool IsCallingAny(MethodDefinition scannedMethod, IEnumerable<TypeDefinition> types)
		{
			return types.Any(t => IsCalling(scannedMethod, t));
		}

		void INorm.Verify(IAssert assert)
		{
			var unmatchedTypes = new HashSet<TypeDefinition>();
			var matchedTypes = GetMatchedTypes();
			foreach (var type in matchedTypes)
			{
				if (norms.Any(x => x(type) == false))
				{
					unmatchedTypes.Add(type);
				}
			}

			assert.IsTrue(unmatchedTypes, t => t.Count == 0, BuildFailureMessage);
		}

		private static bool IsMethodCall(Instruction instruction)
		{
			return instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt;
		}
	}
}