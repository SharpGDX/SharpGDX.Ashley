using SharpGDX.Ashley.Signals;

namespace SharpGDX.Ashley.Tests;

public class SignalTest {
	public SignalTest() {
		Signal<String> signal = new Signal<String>();

		Listener<String> listener = new StringListener();

		signal.add(listener);
		signal.dispatch("Hello World!");
	}

    private class StringListener : Listener<string>
    {
        public void receive(Signal<String> signal, String obj)
        {
            Console.WriteLine("Received event: " + obj);
        }
    }
}
