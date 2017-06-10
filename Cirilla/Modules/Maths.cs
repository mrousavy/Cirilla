using Discord.Commands;
using org.mariuszgromada.math.mxparser;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Maths : ModuleBase {
        [Command("square"), Summary("Squares a number.")]
        public async Task Square([Summary("The number to square.")] double num) {
            try {
                await ReplyAsync($"{num}^2 = {Math.Pow(num, 2)}");
            } catch (Exception ex) {
                await ReplyAsync($"Could not square number! ({ex.Message})");
            }
        }

        [Command("sqrt"), Summary("Calculates the Square Root of a Number.")]
        public async Task Sqrt([Summary("The number to sqrt.")] double num) {
            try {
                await ReplyAsync($"sqrt({num}) = {Math.Sqrt(num)}");
            } catch (Exception ex) {
                await ReplyAsync($"Could not calculate square root! ({ex.Message})");
            }
        }

        [Command("calc"), Summary("Calculates Expressions.")]
        public async Task Calc([Summary("The expression to evaluate.")] params string[] expressions) {
            try {
                string expression = string.Join("", expressions);
                string trimmed = Regex.Replace(expression, @"\s+", "");
                trimmed = trimmed.Replace("\"", "");

                Expression expr = new Expression(trimmed);
                double result = expr.calculate();
                await ReplyAsync($"{expression} = {result}");
            } catch (Exception ex) {
                await ReplyAsync($"Could not calculate Expression! ({ex.Message})");
            }
        }

        [Command("pi"), Summary("Gets PI.")]
        public async Task Pi() {
            await ReplyAsync($"{Math.PI}");
        }
    }
}