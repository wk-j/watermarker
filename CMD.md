## Commands

```
rm /Users/wk/.dotnet/tools/wk-watermarker
cake -target=Pack
dotnet install tool -g wk.Watermarker --source ./publish
wk-watermarker Hello https://static.vecteezy.com/system/resources/previews/000/101/237/non_2x/free-abstract-background-11-vector.jpg
```