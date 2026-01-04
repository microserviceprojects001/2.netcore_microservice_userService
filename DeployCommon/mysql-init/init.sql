-- 创建用户并设置密码
CREATE USER IF NOT EXISTS 'abel'@'%' IDENTIFIED BY 'acsdev312!@';

-- 授予所有权限（可以根据需要调整）
GRANT ALL PRIVILEGES ON *.* TO 'abel'@'%' WITH GRANT OPTION;

-- 刷新权限使更改生效
FLUSH PRIVILEGES;