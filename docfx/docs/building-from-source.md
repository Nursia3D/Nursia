1. Clone this repo.

2. If you want to build FNA version, then you need to clone following repos as well:

   Link|Description
   ----|-----------
   https://github.com/FNA-XNA/FNA|FNA
   https://github.com/rds1983/XNAssets|Asset management library
   https://github.com/rds1983/DigitalRiseModel|3D model library
   https://github.com/FontStashSharp/FontStashSharp|Text rendering library
   https://github.com/rds1983/Myra|UI library

   All repos must be in one folder level.

3. Install [https://github.com/rds1983/efscriptgen](https://github.com/rds1983/efscriptgen): `dotnet tool install --global efscriptgen`

4. Make sure that `mgfxc`(if you want to build MonoGame version) or `fxc`(FNA) is at your PATH environment variable.

5. Go to folder Nursia/Effects and execute `efscriptgen .` 

   It will generate batch scripts required to build the effecs.

6. Go to folder "MonoGameOGL"(for MonoGame) or to folder "FNA" and execute `compile_all.bat`

7. Repeat steps 5-6 at folder "Samples/Nursia.Samples.Character/Effects"

8. Now open the solution file(located at the repo root) in the IDE and build the project.