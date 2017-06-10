using Discord.Commands;
using org.mariuszgromada.math.mxparser;
using System;
using System.Threading.Tasks;


public class Test : ModuleBase {

    [Command("square"), Summary("Squares a number.")]
    public async Task Square([Summary("The number to square.")] double num) {
        try {
            await ReplyAsync($"{num}^2 = {System.Math.Pow(num, 2)}");
        } catch (Exception ex) {
            await ReplyAsync($"Could not square number! ({ex.Message})");
        }
    }
}

[Group("math")]
public class Math : ModuleBase {
    [Command("square"), Summary("Squares a number.")]
    public async Task Square([Summary("The number to square.")] double num) {
        try {
            await ReplyAsync($"{num}^2 = {System.Math.Pow(num, 2)}");
        } catch (Exception ex) {
            await ReplyAsync($"Could not square number! ({ex.Message})");
        }
    }

    [Command("sqrt"), Summary("Calculates the Square Root of a Number.")]
    public async Task Sqrt([Summary("The number to sqrt.")] double num) {
        try {
            await ReplyAsync($"sqrt({num}) = {System.Math.Sqrt(num)}");
        } catch (Exception ex) {
            await ReplyAsync($"Could not calculate square root! ({ex.Message})");
        }
    }

    [Command("calc"), Summary("Calculates Expressions.")]
    public async Task Calc([Summary("The expression to evaluate.")] string expression) {
        try {
            Expression expr = new Expression(expression);
            double result = expr.calculate();
            await ReplyAsync($"{expression} = {result}");
        } catch (Exception ex) {
            await ReplyAsync($"Could not calculate Expression! ({ex.Message})");
        }
    }

    [Command("pi"), Summary("Gets PI.")]
    public async Task Pi() {
        await ReplyAsync($"{System.Math.PI}");
    }
}
