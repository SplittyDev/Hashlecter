[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png)](http://creativecommons.org/publicdomain/zero/1.0/)  
To the extent possible under law, [SplittyDev](https://github.com/SplittyDev) has waived all copyright and related or neighboring rights to Hashlecter.

![HashLecter Logo](http://i.imgur.com/Wt4bxbY.png)  
Free (as in speech) md5 hash collider.

## Using Hashlecter
### Command-line arguments
| Short   | Long          | Type     | Description
|---------|---------------|----------|-------------
| -a      | --alphabet    | Argument | Specify the bruteforce alphabet
| -i      | --input       | Argument | Read hashes from file
| -d      | --dict        | Argument | Perform a dictionary-attack using the specified file
| -s      | --session     | Argument | Specify a session name
| -r      | --rounds      | Argument | Specify hashing rounds
| -l      | --len         | Argument | Specify the max length of the generated string
| -g      | --gen         | Switch   | Generate a hash
| -fupper | --force-upper | Switch   | Force uppercase hashes
|         | --incremental | Switch   | Incremental bruteforce mode
|         | --stdin       | Switch   | Read hashes from stdin
|         | --silent      | Switch   | Don't output anything to stdout
|         | --wizard      | Switch   | Easy configuration
|         | --show        | Switch   | Show results; can be combined with `-s/--session`

Info:  
The default hashing method is md5.  
If you want to generate a hash, just specify the input string using -i/--input

Important:  
The `-r/--rounds` argument doesn't work with variations of hashes.  
If you want to try 6-round md5, do `-r 6 -m md5`, but not `-r 3 -m md5_double`.

#### Experimental/Unstable features
| Short | Long              | Type     | Description
|-------|-------------------|----------|-------------
|       | --exp-lazy-eval   | Switch   | Lazy evaluation of input dictionary
|       | --exp-single-cont | Switch   | Don't stop after finding a collision

### Hashing methods
* . md5
* . md5_double
* ! md5_salted
* ! md5_mybb
* . sha1
* . sha1_double
* . sha256
* . sha256_double
* . jhash

Hashing methods marked with a period use the following format:
```
# This is a comment. It will be ignored.
# Hash 1
6ae99d4d2de5e3cbd29fec87ae7d76eb
# Hash 2
50a39ec9e0e46cf2826eb5745e1c800b
```

Hashing methods marked with an exclamation point use the following format:
```
# This is a comment. It will be ignored.
# Hash 1 : Salt 1
6ae99d4d2de5e3cbd29fec87ae7d76eb:91704826
# Hash 2 : Salt 2
50a39ec9e0e46cf2826eb5745e1c800b:72946193
```

### Examples (mono)
Cracking the md5 hash for "hello" using dictionary "dict":  
```
# mono lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
# mono lecter.exe -d dict -i hashes -r 2
```

Bruteforcing the md5 hash for "hello" using incremental mode and a lowercase alphabet:
```
# mono lecter.exe -a $l --incremental --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

### Examples (Microsoft .Net)
Cracking the md5 hash for "hello" using dictionary "dict":
```
> lecter.exe -d dict --stdin | echo 5d41402abc4b2a76b9719d911017c592
```

Cracking a list "hashes" of 2-round md5-hashed entries using dictionary "dict":
```
> lecter -d dict -i hashes -r 2
```

Bruteforcing the md5 hash for "hello" using incremental mode and a lowercase alphabet:
```
> lecter -a $l --incremental --stdin | echo 5d41402abc4b2a76b9719d911017c592
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
