namespace SwapView;

public readonly struct SwapView : IComparable<SwapView>
{
  public readonly int Pid;
  public readonly double Size;
  public readonly string Comm;

  public SwapView(int p, double s, string c)
  {
    Pid = p;
    Size = s;
    Comm = c;
  }

  public int CompareTo(SwapView s)
  {
    return Size.CompareTo(s.Size);
  }
}

class MainClass
{
  public static string Filesize(double size)
  {
    const string units = "KMGT";
    var left = Math.Abs(size);
    var unit = -1;
    while (left > 1100 && unit < 3)
    {
      left /= 1024;
      unit++;
    }

    if (unit == -1)
    {
      return $"{(int)size:D}B";
    }
    else
    {
      if (size < 0) left = -left;
      return $"{left:F1}{units[unit]:C}iB";
    }
  }

  public static SwapView GetSwapFor(int pid)
  {
    try
    {
      var comm =
        File.ReadAllText($"/proc/{pid:D}/cmdline");
      comm = comm.Replace('\0', ' ');
      if (comm[^1] == ' ')
        comm = comm[..^1];
      var s =
        (from l in File.ReadLines($"/proc/{pid:D}/smaps")
         where l.StartsWith("Swap:")
         select l.Split(' ')).Aggregate<string[], double>(
          0,
          (current, a) => current + Int32.Parse(a[^2]));

      return new SwapView(pid, s * 1024, comm);
    }
    catch (Exception)
    {
      return new SwapView(pid, 0, "");
    }
  }

  public static IEnumerable<SwapView> GetSwap()
  {
    return Directory.EnumerateDirectories("/proc/")
      .Select(fpid => Int32.TryParse(fpid.AsSpan(6), out var pid) ? pid : -1)
      .Where(fpid => fpid != -1)
      .AsParallel()
      .Select(GetSwapFor)
      .Where(s => s.Size > 0)
      .Order();
  }

  public static void Main(string[] args)
  {
    IEnumerable<SwapView> results = GetSwap();
    Console.WriteLine("{0,7} {1,9} {2}", "PID", "SWAP", "COMMAND");
    var t = 0.0;
    foreach (var s in results)
    {
      Console.WriteLine("{0,7:D} {1,9} {2}", s.Pid, Filesize(s.Size), s.Comm);
      t += s.Size;
    }

    Console.WriteLine("Total: {0,10}", Filesize(t));
  }
}
