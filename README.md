[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png)](http://creativecommons.org/publicdomain/zero/1.0/)  
To the extent possible under law, [SplittyDev](https://github.com/SplittyDev) has waived all copyright and related or neighboring rights to Hashlecter. This work is published from: Deutschland.

![HashLecter Logo](http://i.imgur.com/Wt4bxbY.png)  
Free (as in speech) md5 hash collider.

## Using Hashlecter
### Basics
Feeding a list of hashes to lecter: `-i/--input <path/to/file>`  
Accepting input from stdin: `--stdin`  
Specifying the dictionary for a dictionary attack: `-d/--dict <path/to/file>`  
Specifying the hashing method: `-m/--method <hashing_method>`  
Specifying a session name: `-s/--session <session_name>`  
Suppress output to stdout: `--silent`

You don't need to specify a hashing method if you're going to process md5 hashes.

### Hashing methods
* . md5
* . md5_double
* ! md5_salted
* ! md5_mybb
* . sha1

Important:  
Hashing methods marked with an exclamation point  
need to be provided using the following format: `hash:salt`

### Examples (mono)
Cracking the md5 hash for "hello" using dictionary "dict":  
```
# mono lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
# mono lecter.exe -d dict -i hashes -m md5_double
```

### Examples (Microsoft .Net)
Cracking the md5 hash for "hello" using dictionary "dict":
```
> lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
> lecter -d dict -i hashes -m md5_double
```

## Building Hashlecter
### Building under *nix (mono)
```
# git clone https://github.com/SplittyDev/Hashlecter.git hashlecter
# cd hashlecter
# xbuild /p:Configuration=Release
```

### Building under Windows
```
> git clone https://github.com/SplittyDev/Hashlecter.git hashlecter
> cd hashlecter
> msbuild /p:Configuration=Release
```

### Building using Visual Studio / MonoDevelop
Just open the solution file and build the project in Release mode
