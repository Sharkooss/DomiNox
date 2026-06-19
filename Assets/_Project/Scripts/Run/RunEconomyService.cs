namespace DomiNox.Run
{
    public static class RunEconomyService
    {
        public static int AddCredits(RunState run, int amount, CreditSource source)
        {
            if (run == null || amount <= 0)
            {
                return 0;
            }

            var finalAmount = run.DoubleCreditsUntilEndOfFloor ? amount * 2 : amount;
            run.Credits += finalAmount;
            return finalAmount;
        }
    }
}
