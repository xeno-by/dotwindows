@echo off
set CLASSPATH="C:\Projects\Kepler\build\locker\classes\compiler;C:\Projects\Kepler\build\locker\classes\library;C:\Projects\Kepler\build\locker\classes\partest"
cd /D "%PROJECTS%\Kepler\test"
java >con ^
-Xmx1024M >con ^
-Xms64M >con ^
-Dscala.home="C:\Projects\Kepler" >con ^
-Dpartest.javacmd="java" >con ^
-Dpartest.java_options="-Xmx1024M -Xms64M" >con ^
-Dpartest.scalac_options="-deprecation" >con ^
-Dpartest.javac_cmd="javac" >con ^
-Djava.class.path=%CLASSPATH% >con ^
-Dscala.usejavacp=true >con ^
-cp %CLASSPATH% >con ^
-classpath %CLASSPATH% >con ^
scala.tools.partest.nest.NestRunner >con ^
--classpath "C:\Projects\Kepler\build\locker\classes" ^
%*