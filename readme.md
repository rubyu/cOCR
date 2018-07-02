
## cOCR

Cloud OCR utility tool. 

The minimal required options are `-d` and `-k`, which means the target directory and your Google API key.

```bat
cocr -d "PATH_TO_TARGET_DIRECTORY" -k "YOUR_GOOGLE_API_KEY"
```

### Mode
#### FileSystem Watcher Mode

Normally, **cOCR** watchs specified directory by `-d` or `--dir` option, and if it finds a new image file, it will process the file immediately.

#### Bulk Converter Mode

When `-b` option is given, **cOCR** will process all image files except for which of already processed. After that, **cOCR** will automatically be finished.

```bat
cocr -d "PATH_TO_TARGET_DIRECTORY" -k "YOUR_GOOGLE_API_KEY" -b
```

### Google Cloud Vision API

#### Entry Point

The entry point can be overriden by `-e` or `--entry_point` option.
The default entry point is `https://vision.googleapis.com/v1p1beta1/images:annotate?key=`.

#### Language Hints

LanguageHints option can be overriden by `-l` or `--language_hints` option. 
The option value must be separated by commas if multiple values are given. e.g. `-l "en, ja"`. 
The default value is empty.

### Post process

Note: The following options are ignored when `-b` option is given.

#### Clipboard

When `-c` or `--clipboard` option is given, **cOCR** will copy the result to clipboard.

```bat
cocr -d "PATH_TO_TARGET_DIRECTORY" -k "YOUR_GOOGLE_API_KEY" -c
```


### Watch a folder, execute OCR, and copy the result to clipboard.

When `-s` or `--show_result` option is given, **cOCR** will open OCR result html file by the application which associated to the file.

```bat
cocr -d "PATH_TO_TARGET_DIRECTORY" -k "YOUR_GOOGLE_API_KEY" -s
```
