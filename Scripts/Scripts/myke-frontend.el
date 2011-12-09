(defun my-compile-project (buffer &optional callback)
  (myke-invoke "compile" buffer callback))

(defun my-clean-project (buffer &optional callback)
  (myke-invoke "clean" buffer callback))

(defun my-rebuild-project (buffer &optional callback)
  (myke-invoke "rebuild" buffer callback))

(defun my-run-project (buffer &optional callback)
  (myke-invoke "run" buffer callback))

(defun my-repl-project (buffer &optional callback)
  (myke-invoke "repl" buffer callback))

(defun my-run-test-project (buffer &optional callback)
  (myke-invoke "run-test" buffer callback))

(defun my-commit-project (buffer &optional callback)
  (myke-invoke "commit" buffer callback))

(defun my-logall-project (buffer &optional callback)
  (myke-invoke "logall" buffer callback))

(defun my-logthis-project (buffer &optional callback)
  (myke-invoke "logthis" buffer callback))

(defun my-pull-project (buffer &optional callback)
  (myke-invoke "pull" buffer callback))

(defun my-push-project (buffer &optional callback)
  (myke-invoke "push" buffer callback))