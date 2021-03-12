echo Deleting brainflow
cd brainflow/csharp-package/brainflow/brainflow
@RD /S /Q bin
@RD /S /Q obj
echo Deleting BrainflowDataProcessing
cd ../../../../BrainflowDataProcessing
@RD /S /Q bin
@RD /S /Q obj
echo Deleting BrainflowInterfaces
cd ../BrainflowInterfaces
@RD /S /Q bin
@RD /S /Q obj
echo Deleting BrainHatNetwork
cd ../BrainHatNetwork
@RD /S /Q bin
@RD /S /Q obj
echo Deleting EDFfileCSWrapper
cd ../EDFfile/EDFfileCSWrapper
@RD /S /Q bin
@RD /S /Q obj
echo Deleting liblsl-Csharp
cd ../../liblsl-Csharp
@RD /S /Q bin
@RD /S /Q obj
echo Deleting LoggingInterfaces
cd ../LoggingInterfaces
@RD /S /Q bin
@RD /S /Q obj
echo Deleting PlatformHelper
cd ../PlatformHelper
@RD /S /Q bin
@RD /S /Q obj
cd ..
