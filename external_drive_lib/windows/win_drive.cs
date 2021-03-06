﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using external_drive_lib.exceptions;
using external_drive_lib.interfaces;

namespace external_drive_lib.windows
{
    /* note: the main reason we have win drives is so that you can copy from a windows drive to an android drive
     */
    class win_drive : IDrive {

        private string root_;
        private bool valid_ = true;

        public win_drive(DriveInfo di) {
            try {
                root_ = di.RootDirectory.FullName;
            } catch (Exception e) {
                // "bad drive " + di + " : " + e;
                valid_ = false;
            }
        }

        public win_drive(string root) {
            root_ = root;
        }

        public bool is_connected() {
            return true;
        }

        public bool is_available() {
            return true;
        }

        public drive_type type {
            get { return drive_type.internal_hdd; }
        }

        public string root_name {
            get { return root_; }
        }

        public string unique_id {
            get { return root_; }
        } 
        public string friendly_name {
            get { return root_; }
        }

        public IEnumerable<IFolder> folders {
            get { return new DirectoryInfo(root_).EnumerateDirectories().Select(f => new win_folder(root_, f.Name)); }
        }
        public IEnumerable<IFile> files {
            get { return new DirectoryInfo(root_).EnumerateFiles().Select(f => new win_file(root_, f.Name)); }
        }

        public IFile parse_file(string path) {
            var f = try_parse_file(path);
            if ( f == null)
                throw new exception("invalid path " + path);
            return f;
        }

        public IFolder parse_folder(string path) {
            var f = try_parse_folder(path);
            if ( f == null)
                throw new exception("invalid path " + path);
            return f;
        }

        public IFile try_parse_file(string path) {
            path = path.Replace("/", "\\");
            var contains_drive_prefix = path.StartsWith(root_, StringComparison.CurrentCultureIgnoreCase);
            var full = contains_drive_prefix ? path : root_ + path;
            if (File.Exists(full)) {
                var fi = new FileInfo(full);
                return new win_file(fi.DirectoryName, fi.Name);
            }
            return null;
        }

        public IFolder try_parse_folder(string path) {
            path = path.Replace("/", "\\");
            var contains_drive_prefix = path.StartsWith(root_, StringComparison.CurrentCultureIgnoreCase);
            var full = contains_drive_prefix ? path : root_ + path;
            if (Directory.Exists(full)) {
                var fi = new DirectoryInfo(full);
                return new win_folder(fi.Parent.FullName, fi.Name);
            }
            return null;
        }

        public IFolder create_folder(string path) {
            path = path.Replace("/", "\\");
            if (path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);
            var contains_drive_prefix = path.StartsWith(root_, StringComparison.CurrentCultureIgnoreCase);
            if (!contains_drive_prefix)
                path = root_ + path;
            Directory.CreateDirectory(path);

            return parse_folder(path);
        }
    }
}
