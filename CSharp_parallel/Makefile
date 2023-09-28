default: SwapView

SwapView: SwapView.cs
	cd .swapview; \
	dotnet publish -c Release -r linux-x64 --no-self-contained -o ../ ;

clean:
	-rm -f SwapView

run: SwapView
	SwapView
