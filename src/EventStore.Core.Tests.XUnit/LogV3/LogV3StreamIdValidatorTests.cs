﻿using System;
using EventStore.Core.LogV3;
using Xunit;

namespace EventStore.Core.Tests.XUnit.LogV3 {
	public class LogV3StreamIdValidatorTests {
		readonly LogV3StreamIdValidator _sut = new();

		[Fact]
		public void accepts_positive() {
			_sut.Validate(1);
		}

		[Fact]
		public void rejects_zero() {
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				_sut.Validate(0);
			});
		}

		[Fact]
		public void rejects_negative() {
			Assert.Throws<ArgumentOutOfRangeException>(() => {
				_sut.Validate(-1);
			});
		}
	}
}
