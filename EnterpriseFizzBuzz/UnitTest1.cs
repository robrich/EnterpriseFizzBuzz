using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FizzBuzzEnterprise
{
	/// <summary>
	/// Defines the interface for playing a Fizz Buzz game.
	/// </summary>
	public interface IFizzBuzzEngine
	{
		/// <summary>
		/// Gets the output from a single round in a Fizz Buzz game.
		/// </summary>
		string GetString(int number);
	}

	/// <summary>
	/// Defines various rule sets for Fizz Buzz games.
	/// </summary>
	public enum FizzBuzzRuleSet
	{
		/// <summary>
		/// The standard game - based on the divisibility of a number.
		/// </summary>
		FizzBuzzDivisible,

		/// <summary>
		/// The standard game with additional substitutions for divisibility.
		/// </summary>
		FizzBuzzBoomBangCrashDivisible,

		/// <summary>
		/// A game based on the digits contained in a number.
		/// </summary>
		FizzBuzzDigits,

		/// <summary>
		/// A combination of FizzBuzzDivisible and FizzBuzzDigits.
		/// </summary>
		FizzBuzzDivisibleOrDigits
	}



	// TODO: use IoC to inject in dependences and remove the factory
	public static class FizzBuzzEngineFactory
	{
		public static IFizzBuzzEngine Create(FizzBuzzRuleSet ruleSet)
		{
			IFizzBuzzSolverFactory factory = new FizzBuzzSolverFactory();
			IFizzBuzzSolver solver = factory.GetSolver(ruleSet);
			return new FizzBuzzEngine(solver);
		}
	}

public interface IFizzBuzzSolverFactory
{
	IFizzBuzzSolver GetSolver(FizzBuzzRuleSet ruleSet);
}

// instance so we can put it in the IoC container
public class FizzBuzzSolverFactory : IFizzBuzzSolverFactory
{
	// TODO: when FizzBuzzEngineFactory is gone,
	// refactor to GetSolver<FizzBuzzRuleSet>()
	// and use service locator to resolve other dependencies.
	public IFizzBuzzSolver GetSolver(FizzBuzzRuleSet ruleSet)
	{
		IFizzBuzzSolver solver = null;
		switch (ruleSet) {
			/// <summary>
			/// The standard game - based on the divisibility of a number.
			/// </summary>
			case FizzBuzzRuleSet.FizzBuzzDivisible:
				solver = new FizzBuzzDivisibleSolver();
				break;
			/// <summary>
			/// The standard game with additional substitutions for divisibility.
			/// </summary>
			case FizzBuzzRuleSet.FizzBuzzBoomBangCrashDivisible:
				solver = new FizzBuzzCrashDivisbleSolver();
				break;
			/// <summary>
			/// A game based on the digits contained in a number.
			/// </summary>
			case FizzBuzzRuleSet.FizzBuzzDigits:
				solver = new FizzBuzzDigitsSolver();
				break;
			/// <summary>
			/// A combination of FizzBuzzDivisible and FizzBuzzDigits.
			/// </summary>
			case FizzBuzzRuleSet.FizzBuzzDivisibleOrDigits:
				solver = new FizzBuzzDivisibleOrDigitsSolver(new FizzBuzzDivisibleSolver(), new FizzBuzzDigitsSolver());
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(ruleSet), $"{ruleSet} is not a handled {nameof(ruleSet)}"); // <-- ASSUME: C# 7 compiler
		}
		return solver;
	}
}

public class FizzBuzzEngine : IFizzBuzzEngine
{
	private readonly IFizzBuzzSolver solver;

	public FizzBuzzEngine(IFizzBuzzSolver solver)
	{
		this.solver = solver;
	}

	public string GetString(int number) => solver.Solve(number) ?? number.ToString(); // <-- ASSUME: C# 7 compiler
}

public interface IFizzBuzzSolver
{
	string Solve(int number);
}

/// <summary>
/// The standard game - based on the divisibility of a number.
/// </summary>
public class FizzBuzzDivisibleSolver : IFizzBuzzSolver
{

	public virtual string Solve(int number)
	{
		// string used here because we're optimizing for legibility / maintainability
		// TODO: consider StringBuilder if optimizing for performance
		string result = null;
		if (number % 3 == 0) {
			result += "Fizz";
		}
		if (number % 5 == 0) {
			result += "Buzz";
		}
		return result;
	}

}

/// <summary>
/// The standard game with additional substitutions for divisibility.
/// </summary>
public class FizzBuzzCrashDivisbleSolver : FizzBuzzDivisibleSolver
{

	public override string Solve(int number)
	{
		string result = base.Solve(number); // use standard game
		if (number % 7 == 0) {
			result += "Boom";
		}
		if (number % 11 == 0) {
			result += "Bang";
		}
		if (number % 13 == 0) {
			result += "Crash";
		}
		return result;
	}

}

/// <summary>
/// A game based on the digits contained in a number.
/// </summary>
public class FizzBuzzDigitsSolver : IFizzBuzzSolver
{

	/* more terse, more functional, but less legible:
	public string Solve(int number)
	{
		Func<int, string> solver = (int digit) => digit == 3 ? "Fizz" : digit == 5 ? "Buzz" : null;
		string result = string.Join("", (
			from n in number.ToString().ToList()
			let i = int.Parse($"{n}") // char -> int
			select solver(i)
		).ToList());
	}
	*/

	public string Solve(int number)
	{
		List<int> digits = GetDigits(number);
		string result = null;
		foreach (int digit in digits) {
			result += GetResultForDigit(digit);
		}
		if (string.IsNullOrEmpty(result)) {
			return null;
		}
		return result;
	}

	// ASSUME: C# 7 compiler
	private List<int> GetDigits(int number) => (
		from n in number.ToString().ToList()
		let i = int.Parse($"{n}") // char -> int
			select i
	).ToList();

	private string GetResultForDigit(int digit)
	{
		if (digit == 3) {
			return "Fizz";
		}
		if (digit == 5) {
			return "Buzz";
		}
		return null;
	}

}

	/// <summary>
	/// A combination of FizzBuzzDivisible and FizzBuzzDigits.
	/// </summary>
	public class FizzBuzzDivisibleOrDigitsSolver : IFizzBuzzSolver
	{
		private readonly FizzBuzzDivisibleSolver fizzBuzzDivisibleSolver;
		private readonly FizzBuzzDigitsSolver fizzBuzzDigitsSolver;

		public FizzBuzzDivisibleOrDigitsSolver(FizzBuzzDivisibleSolver fizzBuzzDivisibleSolver, FizzBuzzDigitsSolver fizzBuzzDigitsSolver)
		{
			this.fizzBuzzDivisibleSolver = fizzBuzzDivisibleSolver;
			this.fizzBuzzDigitsSolver = fizzBuzzDigitsSolver;
		}

		public string Solve(int number) {
			string result = fizzBuzzDivisibleSolver.Solve(number) + fizzBuzzDigitsSolver.Solve(number);
			return string.IsNullOrEmpty(result) ? null : result;
		}

	}

}
// TODO: move to separate assembly so we don't deploy test code in production
// TODO: consider SpecFlow so test data reads like requirements above
namespace FizzBuzzEnterpriseTests
{
	using FizzBuzzEnterprise;
	using Xunit;
	using FluentAssertions;

	public class SmokeTests
	{
		[Fact]
		public void FizzBuzzDivisible_Works()
		{

			// Arrange
			int number = 5;
			string expected = "Buzz";

			// Act
			IFizzBuzzEngine engine = FizzBuzzEngineFactory.Create(FizzBuzzRuleSet.FizzBuzzDivisible);
			string actual = engine.GetString(number);

			// Assert
			actual.Should().Be(expected);

		}

		// JIRA 20134: `null + null` => ""
		[Fact]
		public void FizzBuzzDivisibleOrDigitsSolver_Works()
		{

			// Arrange
			int number = 92;
			string expected = "92";

			// Act
			IFizzBuzzEngine engine = FizzBuzzEngineFactory.Create(FizzBuzzRuleSet.FizzBuzzDivisibleOrDigits);
			string actual = engine.GetString(number);

			// Assert
			actual.Should().Be(expected);

		}

	}

	/// <summary>
	/// The standard game - based on the divisibility of a number.
	/// </summary>
	public class FizzBuzzDivisibleSolverTests
	{
		[Theory]
		[InlineData(12, "Fizz")]
		[InlineData(10, "Buzz")]
		[InlineData(15, "FizzBuzz")]
		[InlineData(14, null)]
		public void Test_FizzBuzzDivisibleSolver_Works(int number, string expected)
		{

			// Arrange            

			// Act
			FizzBuzzDivisibleSolver engine = new FizzBuzzDivisibleSolver();
			string actual = engine.Solve(number);

			// Assert
			actual.Should().Be(expected);

		}

	}

	/// <summary>
	/// The standard game with additional substitutions for divisibility.
	/// </summary>
	public class FizzBuzzCrashDivisbleSolverTests
	{
		[Theory]
		[InlineData(28, "Boom")]
		[InlineData(33, "FizzBang")]
		[InlineData(65, "BuzzCrash")]
		[InlineData(64, null)]
		public void Test_FizzBuzzCrashDivisbleSolver_Works(int number, string expected)
		{

			// Arrange            

			// Act
			FizzBuzzCrashDivisbleSolver engine = new FizzBuzzCrashDivisbleSolver();
			string actual = engine.Solve(number);

			// Assert
			actual.Should().Be(expected);

		}

	}

	/// <summary>
	/// A game based on the digits contained in a number.
	/// </summary>
	public class FizzBuzzDigitsSolverTests
	{
		[Theory]
		[InlineData(13, "Fizz")]
		[InlineData(51, "Buzz")]
		[InlineData(365, "FizzBuzz")]
		[InlineData(532, "BuzzFizz")]
		[InlineData(325395, "FizzBuzzFizzBuzz")]
		[InlineData(91, null)]
		public void Test_FizzBuzzDigitsSolver_Works(int number, string expected)
		{

			// Arrange            

			// Act
			FizzBuzzDigitsSolver engine = new FizzBuzzDigitsSolver();
			string actual = engine.Solve(number);

			// Assert
			actual.Should().Be(expected);

		}

	}

	/// <summary>
	/// A combination of FizzBuzzDivisible and FizzBuzzDigits.
	/// </summary>
	public class FizzBuzzDivisibleOrDigitsSolverTests
	{
		[Theory]
		[InlineData(12, "Fizz")]
		[InlineData(10, "Buzz")]
		[InlineData(13, "Fizz")]
		[InlineData(53, "BuzzFizz")]
		[InlineData(30, "FizzBuzzFizz")]
		[InlineData(51435, "FizzBuzzBuzzFizzBuzz")]
		[InlineData(92, null)]
		public void Test_FizzBuzzDivisibleOrDigitsSolver_Works(int number, string expected)
		{

			// Arrange            

			// Act
			FizzBuzzDivisibleOrDigitsSolver engine = new FizzBuzzDivisibleOrDigitsSolver(new FizzBuzzDivisibleSolver(), new FizzBuzzDigitsSolver());
			string actual = engine.Solve(number);

			// Assert
			actual.Should().Be(expected);

		}

	}

}

/*
namespace FizzBuzzEnterprise
{

	class Program
	{
		static void Main(string[] args)
		{
			var ruleSet = (FizzBuzzRuleSet)Enum.Parse(
				typeof(FizzBuzzRuleSet), Console.ReadLine());
			var number = int.Parse(Console.ReadLine());

			IFizzBuzzEngine engine = FizzBuzzEngineFactory.Create(ruleSet);
			var result = engine.GetString(number);

			using (var writer = new StreamWriter(
				Environment.GetEnvironmentVariable("OUTPUT_PATH"), true)) {
				writer.Write(result);
			}
		}
	}
}
*/