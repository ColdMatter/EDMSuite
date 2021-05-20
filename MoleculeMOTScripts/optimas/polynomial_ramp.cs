public void AddPolynomialRamp(string channel, int startTime, int steps, int finalTime,
     double w0, double w1, double w2, double w3)
    {
        if (PatternLength > finalTime)
        {
            double startValue = GetValue(channel, startTime);
            double stepSize = (finalTime - startTime ) / steps;
            for (int i = 0; i < steps; i++)
            {
                if (AnalogPatterns[channel].ContainsKey(startTime + i) == false)
                {
                    double timeStep = (i + 1) * stepSize;
                    double order1 = w0 * timeStep;
                    double order2 = w1 * timeStep * timeStep;
                    double order3 = w2 * timeStep * timeStep * timeStep;
                    double order3 = w3 * timeStep * timeStep * timeStep * timeStep;
                    double totalValue = startValue + order1 + order2 + order3 + order4;
                    AddAnalogValue(channel, startTime + i, totalValue);
                }
                else
                {
                    throw new ConflictInPatternException();
                }
            }
        }
        else
        {
            throw new InsufficientPatternLengthException();
        }
    }
