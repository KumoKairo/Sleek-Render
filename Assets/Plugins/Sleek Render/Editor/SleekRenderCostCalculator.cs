public static class SleekRenderCostCalculator
{
    private static int maxBloomCost = 88466;
    private static int maxColorizeCost = 130000;
    private static int maxVignetteCost = 700;

    public static int GetMaxBloomCost()
    {
        return maxBloomCost;
    }

    public static int GetMaxColorizeCost()
    {
        return maxColorizeCost;
    }

    public static int GetMaxVignetteCost()
    {
        return maxVignetteCost;
    }

    public static int GetBloomCost(bool isEnabled, int yRation)
    {
        if (isEnabled)
        {
            int result = 13466; // glBlindframebuffer cost
            switch (yRation)
            { // use Horizontal + Vertical Blur values
                case 32:
                    return result + 20200;
                case 64: // avergae between 32 and 128
                    return result + 26600;
                case 128:
                    return result + 33000;
                default:
                    return result;
            }
        }
        return 0;
    }

    public static int GetBloomCost(bool isEnabled, int xRation, int yRation)
    {
        if (isEnabled)
        {
            int result = 13466; // glBlindframebuffer cost
            switch (xRation)
            { // use Horizontal Blur values
                case 32:
                    result += 18000;
                    break;
                case 64: // avergae between 32 and 128
                    result += 28500;
                    break;
                case 128:
                    result += 39000;
                    break;
                default:
                    break;
            }
            switch (yRation)
            { // use Vertical Blur values
                case 32:
                    return result + 5300;
                case 64: // avergae between 32 and 128
                    return result + 20650;
                case 128:
                    return result + 36000;
                default:
                    return result;
            }
        }
        return 0;
    }

    public static int GetColorizeCost(bool isEnabled)
    {
        if (isEnabled)
        { // use Compose with colorize - without colorize
            return 130000;
        }
        return 0;
    }

    public static int GetVignetteCost(bool isEnabled)
    {
        if (isEnabled)
        { // use Precompose with vignette - without vignette
            return 700;
        }
        return 0;
    }

    public static int GetTotalCost(int bloom, int colorize, int vignette)
    {
        if (bloom + colorize + vignette == 0)
        {
            return 0;
        }
        int downsample = 400000, precompose = 6000, compose = 1050000; // approximate cost
        int glBlindframebuffer = 20199;
        return downsample + bloom + colorize + vignette + precompose + compose + glBlindframebuffer;
    }
}
