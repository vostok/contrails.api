#!/usr/bin/env bash
curl https://github.com/vostok/cement/releases/download/v1.0.21-vostok/cement.tar.gz -O -J -L
mkdir ../cement
tar -zxvf cement.tar.gz -C ../cement
cd ..
mono ./cement/cm.exe init
curl https://raw.githubusercontent.com/vostok/cement-modules/master/settings -O -J -L
/bin/cp ./settings ~/.cement/
cd $OLDPWD
mono ../cement/cm.exe update-deps
mono ../cement/cm.exe build-deps
mono ../cement/cm.exe build
