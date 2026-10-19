using _test_common.Framework;
using Ans.Net10.Common;

namespace _test_common.Demos;

public static class ImageResizeHelperDemo
{
    public static Task Run()
    {
        DemoConsole.Part("ImageResizeHelper / ImageOrientationEnum / ImageShiftEnum");
        OrientationBoundaries();
        ScaleChecks();
        ScaleInsideChecks();
        ScaleAroundChecks();
        ScaleAverageChecks();
        ScaleToWidthHeightChecks();
        CropStartChecks();
        CropStartByShiftChecks();
        TransparentMaskCheck();
        return Task.CompletedTask;
    }

    private static void OrientationBoundaries()
    {
        DemoConsole.Note("Ориентация и IsNearSquare — границы 0.9 и 1.1");

        DemoConsole.Case("(0, 0): Ratio NaN, Square, IsNearSquare=true", () =>
        {
            var h = new ImageResizeHelper(0, 0);
            return (float.IsNaN(h.Ratio), h.Orientation, h.IsNearSquare);
        }, (true, ImageOrientationEnum.Square, true));

        DemoConsole.Case("(0, 100): Ratio 0, Portrait", () =>
            new ImageResizeHelper(0, 100).Orientation, ImageOrientationEnum.Portrait);

        DemoConsole.Case("(100, 0): Ratio +∞, Landscape", () =>
            new ImageResizeHelper(100, 0).Orientation, ImageOrientationEnum.Landscape);

        DemoConsole.Case("(1000, 1000): Square", () =>
            new ImageResizeHelper(1000, 1000).Orientation, ImageOrientationEnum.Square);

        DemoConsole.Case("(1100, 1000): Ratio=1.1 → НЕ near-square", () =>
            new ImageResizeHelper(1100, 1000).IsNearSquare, false);

        DemoConsole.Case("(1099, 1000): Ratio=1.099 → near-square", () =>
            new ImageResizeHelper(1099, 1000).IsNearSquare, true);

        DemoConsole.Case("(900, 1000): Ratio=0.9 → НЕ near-square", () =>
            new ImageResizeHelper(900, 1000).IsNearSquare, false);

        DemoConsole.Case("(901, 1000): Ratio=0.901 → near-square", () =>
            new ImageResizeHelper(901, 1000).IsNearSquare, true);
    }

    private static void ScaleChecks()
    {
        DemoConsole.Note("Scale — линейный коэффициент");
        DemoConsole.Case("Scale(0.5f) 1000×800 → 500×400", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.Scale(0.5f);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "500×400");

        DemoConsole.Case("Scale(2f) 1000×800 → 2000×1600", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.Scale(2f);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "2000×1600");

        DemoConsole.Case("Scale(0) → 0×0", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.Scale(0f);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "0×0");

        DemoConsole.Note("Scale(-1) — интересный кейс: RoundToUInt от отрицательного");
        DemoConsole.Case("Scale(-1) — что вернёт", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.Scale(-1f);
            return $"{h.NewWidth}×{h.NewHeight}";
        });
    }

    private static void ScaleInsideChecks()
    {
        DemoConsole.Note("ScaleInside — впиcaть в рамку");
        DemoConsole.Case("1000×1000 в 500×500 → 500×500", () =>
        {
            var h = new ImageResizeHelper(1000, 1000); h.ScaleInside(500, 500, false);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "500×500");

        DemoConsole.Case("noIncrease: 500×500 в 1000×1000 → не увеличивается", () =>
        {
            var h = new ImageResizeHelper(500, 500); h.ScaleInside(1000, 1000, true);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "500×500");

        DemoConsole.Case("без noIncrease: 500×500 в 1000×1000 → 1000×1000", () =>
        {
            var h = new ImageResizeHelper(500, 500); h.ScaleInside(1000, 1000, false);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "1000×1000");
    }

    private static void ScaleAroundChecks()
    {
        DemoConsole.Note("ScaleAround — заполнить рамку (кроп)");
        DemoConsole.Case("1000×500 в 200×200 → 400×200", () =>
        {
            var h = new ImageResizeHelper(1000, 500); h.ScaleAround(200, 200);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "400×200");
    }

    private static void ScaleAverageChecks()
    {
        DemoConsole.Note("ScaleAverage — усреднение вокруг стороны");
        DemoConsole.Case("1000×500, side=200", () =>
        {
            var h = new ImageResizeHelper(1000, 500); h.ScaleAverage(200);
            return $"{h.NewWidth}×{h.NewHeight}";
        });
        DemoConsole.Note("ScaleAverage(0) — потенциальное деление на 0 внутри?");
        DemoConsole.Case("side=0", () =>
        {
            var h = new ImageResizeHelper(1000, 500); h.ScaleAverage(0);
            return $"{h.NewWidth}×{h.NewHeight}";
        });
    }

    private static void ScaleToWidthHeightChecks()
    {
        DemoConsole.Note("ScaleToWidth / ScaleToHeight");
        DemoConsole.Case("ScaleToWidth(500, noIncrease=false) 1000×800 → 500×400", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.ScaleToWidth(500, false);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "500×400");

        DemoConsole.Case("ScaleToWidth(5000, noIncrease=true) 1000×800 → без увеличения", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.ScaleToWidth(5000, true);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "1000×800");

        DemoConsole.Case("ScaleToHeight(400, noIncrease=false) 1000×800 → 500×400", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.ScaleToHeight(400, false);
            return $"{h.NewWidth}×{h.NewHeight}";
        }, "500×400");

        DemoConsole.Note("ScaleToWidth(0) — интересный кейс");
        DemoConsole.Case("width=0", () =>
        {
            var h = new ImageResizeHelper(1000, 800); h.ScaleToWidth(0, false);
            return $"{h.NewWidth}×{h.NewHeight}";
        });
    }

    private static void CropStartChecks()
    {
        DemoConsole.Note("GetCropStart — границы");
        DemoConsole.Case("length=100, crop=100 → 0", () => ImageResizeHelper.GetCropStart(100u, 100u, (byte)50), 0u);
        DemoConsole.Case("length=100, crop=200 → 0", () => ImageResizeHelper.GetCropStart(100u, 200u, (byte)50), 0u);
        DemoConsole.Case("length=100, crop=50, pct=0", () => ImageResizeHelper.GetCropStart(100u, 50u, (byte)0), 0u);
        DemoConsole.Case("length=100, crop=50, pct=50", () => ImageResizeHelper.GetCropStart(100u, 50u, (byte)50), 25u);
        DemoConsole.Case("length=100, crop=50, pct=100", () => ImageResizeHelper.GetCropStart(100u, 50u, (byte)100), 50u);
        DemoConsole.Case("pct=200 → clamp до 100", () => ImageResizeHelper.GetCropStart(100u, 50u, (byte)200), 50u);
        DemoConsole.Case("pct=255 → clamp до 100", () => ImageResizeHelper.GetCropStart(100u, 50u, (byte)255), 50u);
    }

    private static void CropStartByShiftChecks()
    {
        DemoConsole.Note("GetCropStart(shift) — предопределённые");
        DemoConsole.Case("Start (0%)", () => ImageResizeHelper.GetCropStart(100u, 50u, ImageShiftEnum.Start), 0u);
        DemoConsole.Case("Center (50%)", () => ImageResizeHelper.GetCropStart(100u, 50u, ImageShiftEnum.Center), 25u);
        DemoConsole.Case("End (100%)", () => ImageResizeHelper.GetCropStart(100u, 50u, ImageShiftEnum.End), 50u);
        DemoConsole.Case("StartMiddle (25%)", () => ImageResizeHelper.GetCropStart(100u, 60u, ImageShiftEnum.StartMiddle), 10u);
        DemoConsole.Case("EndMiddle (75%)", () => ImageResizeHelper.GetCropStart(100u, 60u, ImageShiftEnum.EndMiddle), 30u);
        DemoConsole.Case("crop >= length → 0", () => ImageResizeHelper.GetCropStart(50u, 100u, ImageShiftEnum.Center), 0u);
    }

    private static void TransparentMaskCheck()
    {
        DemoConsole.Note("TRANSPARENT_MASK");
        DemoConsole.Case("длина == 192", () => ImageResizeHelper.TRANSPARENT_MASK.Length, 192);
        DemoConsole.Case("первый байт == 0x99", () => ImageResizeHelper.TRANSPARENT_MASK[0], (byte)0x99);
        DemoConsole.Case("12-й байт == 0xcc", () => ImageResizeHelper.TRANSPARENT_MASK[12], (byte)0xcc);
    }
}