#!/usr/bin/perl
use strict;
use warnings;

use Win32::OLE;
use Win32::OLE::Variant;
use Win32::OLE::Const;
use Data::Dumper;
use File::Spec;

my @filetable = ();
while (glob("langs/*.xls"))
{
   push (@filetable,  File::Spec->rel2abs( $_ ));
}


# set perl's OLE module to return Unicode
# if you don't do this perl will convert to the current locale
Win32::OLE->Option(CP => Win32::OLE::CP_UTF8);

# get the Excel Application
my $g_xl = Win32::OLE->new('Excel.Application', sub {$_[0]->Quit;});

# get Excel's constants
my $xl = Win32::OLE::Const->Load($g_xl);
#print Dumper ($xl);

map {
   my $srcFilename = $_;
   my $dstFilename = substr($srcFilename, 0, length($srcFilename) - 3) . "xml";

   print "converting $srcFilename to $dstFilename\n";
   # open the file. Note that excel requires a FULL path and it requires
   # backslashses
   my $xlFileHandle = $g_xl->Workbooks->Open($srcFilename);

   # look up sheet1 by name.
   if (-e $dstFilename)
   {
      unlink ($dstFilename);
   }
   $xlFileHandle->SaveAs(
      $dstFilename,
      $xl->{'xlXMLSpreadsheet'}
      );

   $xlFileHandle->Close;

   undef $xlFileHandle;
} @filetable;

undef $xl;

#$g_xl->DisplayAlerts() = 0;
$g_xl->Quit;

undef $g_xl;



