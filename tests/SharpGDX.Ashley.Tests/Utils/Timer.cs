using SharpGDX.Shims;
using SharpGDX.Utils;

namespace SharpGDX.Ashley.Tests.Utils;

/**
 * A simple Timer class that let's you measure multiple times and are identified via an id.
 * @author Stefan Bachmann
 */
public class Timer
{
    private readonly ObjectMap<string, long> times;

    public Timer()
    {
        times = new ObjectMap<string, long>();
    }

    /**
     * Start tracking a time with name as id.
     * @param name The timer's id
     */
    public void start(string name)
    {
        times.put(name, TimeUtils.currentTimeMillis());
    }

    /**
     * Stop tracking the specified id
     * @param name The timer's id
     * @return the elapsed time
     */
    public long stop(string name)
    {
        if (times.containsKey(name))
        {
            var startTime = times.remove(name);
            return TimeUtils.currentTimeMillis() - startTime;
        }

        throw new RuntimeException("Timer id doesn't exist.");
    }
}