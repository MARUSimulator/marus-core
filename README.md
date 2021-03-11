# unityrossample-unity
Sample project for unity and ros integration

## Installation

Download repository and run it inside Unity 2019.4.x(LTS)

If there are errors:
```
Library\PackageCache\com.unity.robotics.ros-tcp-connector@2b09bf7e77\Runtime\ROSGeometry\ROSVector3.cs(200,104): error CS1501: No overload for method 'ToString' takes 2 arguments
```
just delete `formatProvider` as an argument, so the line looks like 
```
internalVector.ToString(format);
```

Do this for all errors of this form.


## Test

Go to `Robotics` -> `ROS Settings` and configure where the ROS server endpoint is. 
Ensure that the ROS server is up and running.

## TODO

Add multiple scenes and load the scene you want to test

