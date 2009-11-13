﻿using System.Collections.Generic;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;
using System;

namespace Autofac.Tests.Builder
{
    public class G<T>
    {
#pragma warning disable 169
        T unused;
#pragma warning restore 169

        public G(int i)
        {
            I = i;
        }

        public int I { get; private set; }
    }

    [TestFixture]
    public class GenericRegistrationBuilderFixture
    {
        [Test]
        public void BuildGenericRegistration()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(List<>))
                .As(typeof(ICollection<>));
            var c = cb.Build();

            ICollection<int> coll = c.Resolve<ICollection<int>>();
            ICollection<int> coll2 = c.Resolve<ICollection<int>>();

            Assert.IsNotNull(coll);
            Assert.IsNotNull(coll2);
            Assert.AreNotSame(coll, coll2);
            Assert.IsTrue(coll.GetType().GetGenericTypeDefinition() == typeof(List<>));
        }

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(List<>)).As(typeof(IEnumerable<>));
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(IEnumerable<int>)), out cr));
            Assert.AreEqual(typeof(List<int>), cr.Activator.LimitType);
        }

        [Test]
        public void FiresPreparing()
        {
            int preparingFired = 0;
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(List<>))
                .As(typeof(IEnumerable<>))
                .UsingConstructor()
                .OnPreparing(e => ++preparingFired);
            var container = cb.Build();
            container.Resolve<IEnumerable<int>>();
            Assert.AreEqual(1, preparingFired);
        }

        [Test]
        public void SuppliesParameterToConcreteComponent()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(G<>)).WithParameter(new NamedParameter("i", 42));
            var c = cb.Build();
            var g = c.Resolve<G<string>>();
            Assert.AreEqual(42, g.I);
        }

        [Test]
        public void GenericCircularityAvoidedWithUsingContstructor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(List<>))
                .As(typeof(IEnumerable<>))
                .UsingConstructor(new Type[] { });
            var container = builder.Build();
            var list = container.Resolve<IEnumerable<int>>();
        }
    }
}
