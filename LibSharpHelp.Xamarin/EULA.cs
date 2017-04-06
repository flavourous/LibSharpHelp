﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTag.Backend;
using Xamarin.Forms;

namespace LibXamHelp
{
    public class EULA
    {
        #region EULA
        const String eula = @"{1} v{0}
Copyright (c) 2016 {2}

*** END USER LICENSE AGREEMENT ***

IMPORTANT: PLEASE READ THIS LICENSE CAREFULLY BEFORE USING THIS SOFTWARE.

1. LICENSE

By receiving, opening the file package, and/or using {1} v{0}(""Software"") containing this software, you agree that this End User User License Agreement(EULA) is a legally binding and valid contract and agree to be bound by it. You agree to abide by the intellectual property laws and all of the terms and conditions of this Agreement.

Unless you have a different license agreement signed by {2} your use of {1} v{0} indicates your acceptance of this license agreement and warranty.

Subject to the terms of this Agreement, {2} grants to you a limited, non-exclusive, non-transferable license, without right to sub-license, to use {1} v{0} in accordance with this Agreement and any other written agreement with {2}.{2} does not transfer the title of {1} v{0} to you; the license granted to you is not a sale.This agreement is a binding legal agreement between {2} and the purchasers or users of {1} v{0}.

If you do not agree to be bound by this agreement, remove {1} v{0} from your computer now and, if applicable, promptly return to {2} by mail any copies of {1} v{0} and related documentation and packaging in your possession.

2. DISTRIBUTION

{1} v{0} and the license herein granted shall not be copied, shared, distributed, re-sold, offered for re-sale, transferred or sub-licensed in whole or in part except that you may make one copy for archive purposes only.For information about redistribution of {1} v{0} contact {2}.

3. USER AGREEMENT

3.1 Use

Your license to use {1} v{0} is limited to the number of licenses purchased by you.You shall not allow others to use, copy or evaluate copies of {1} v{0}.

3.2 Use Restrictions


You shall use {1} v{0} in compliance with all applicable laws and not for any unlawful purpose.Without limiting the foregoing, use, display or distribution of {1} v{0} together with material that is pornographic, racist, vulgar, obscene, defamatory, libelous, abusive, promoting hatred, discriminating or displaying prejudice based on religion, ethnic heritage, race, sexual orientation or age is strictly prohibited.


Each licensed copy of {1} v{0} may be used on one single computer location by one user. Use of {1} v{0} means that you have loaded, installed, or run {1} v{0} on a computer or similar device.If you install {1} v{0} onto a multi-user platform, server or network, each and every individual user of {1} v{0} must be licensed separately.

You may make one copy of {1} v{0} for backup purposes, providing you only have one copy installed on one computer being used by one person.Other users may not use your copy of {1} v{0}.The assignment, sublicense, networking, sale, or distribution of copies of {1} v{0} are strictly forbidden without the prior written consent of {2}.It is a violation of this agreement to assign, sell, share, loan, rent, lease, borrow, network or transfer the use of {1} v{0}.If any person other than yourself uses {1} v{0} registered in your name, regardless of whether it is at the same time or different times, then this agreement is being violated and you are responsible for that violation!

3.3 Copyright Restriction


This Software contains copyrighted material, trade secrets and other proprietary material. You shall not, and shall not attempt to, modify, reverse engineer, disassemble or decompile {1} v{0}. Nor can you create any derivative works or other works that are based upon or derived from {1} v{0} in whole or in part.

{2}'s name, logo and graphics file that represents {1} v{0} shall not be used in any way to promote products developed with {1} v{0} . {2} retains sole and exclusive ownership of all right, title and interest in and to {1} v{0} and all Intellectual Property rights relating thereto.


Copyright law and international copyright treaty provisions protect all parts of {1} v{0}, products and services.No program, code, part, image, audio sample, or text may be copied or used in any way by the user except as intended within the bounds of the single user program.All rights not expressly granted hereunder are reserved for {2}.

3.4 Limitation of Responsibility

You will indemnify, hold harmless, and defend {2} , its employees, agents and distributors against any and all claims, proceedings, demand and costs resulting from or in any way connected with your use of {2}'s Software.


In no event (including, without limitation, in the event of negligence) will {2} , its employees, agents or distributors be liable for any consequential, incidental, indirect, special or punitive damages whatsoever (including, without limitation, damages for loss of profits, loss of use, business interruption, loss of information or data, or pecuniary loss), in connection with or arising out of or related to this Agreement, {1} v{0} or the use or inability to use {1} v{0} or the furnishing, performance or use of any other matters hereunder whether based upon contract, tort or any other theory including negligence.

{2}'s entire liability, without exception, is limited to the customers' reimbursement of the purchase price of the Software(maximum being the lesser of the amount paid by you and the suggested retail price as listed by {2} ) in exchange for the return of the product, all copies, registration papers and manuals, and all materials that constitute a transfer of license from the customer back to {2}.

3.5 Warranties

Except as expressly stated in writing, {2} makes no representation or warranties in respect of this Software and expressly excludes all other warranties, expressed or implied, oral or written, including, without limitation, any implied warranties of merchantable quality or fitness for a particular purpose.

3.6 Governing Law

This Agreement shall be governed by the law of the United Kingdom applicable therein.You hereby irrevocably attorn and submit to the non-exclusive jurisdiction of the courts of United Kingdom therefrom.If any provision shall be considered unlawful, void or otherwise unenforceable, then that provision shall be deemed severable from this License and not affect the validity and enforceability of any other provisions.

3.7 Termination

Any failure to comply with the terms and conditions of this Agreement will result in automatic and immediate termination of this license.Upon termination of this license granted herein for any reason, you agree to immediately cease use of {1} v{0} and destroy all copies of {1} v{0} supplied under this Agreement.The financial obligations incurred by you shall survive the expiration or termination of this license.

4. DISCLAIMER OF WARRANTY

THIS SOFTWARE AND THE ACCOMPANYING FILES ARE SOLD ""AS IS"" AND WITHOUT WARRANTIES AS TO PERFORMANCE OR MERCHANTABILITY OR ANY OTHER WARRANTIES WHETHER EXPRESSED OR IMPLIED. THIS DISCLAIMER CONCERNS ALL FILES GENERATED AND EDITED BY {1} v{0} AS WELL.

5. CONSENT OF USE OF DATA

You agree that {2} may collect and use information gathered in any manner as part of the product support services provided to you, if any, related to {1} v{0}.{2} may also use this information to provide notices to you which may be of use or interest to you.
 	"; // {0} - appversion
        const String SQLITE_NET_EULA = @"Copyright (c) 2009-2016 Krueger Systems, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
";
        #endregion

        static String BuildEULA(IAppInfo i)
        {
            var our = String.Format(eula, i.AppVersion, i.AppName, "FlavourWare");
            var nl = Environment.NewLine;
            return String.Format(@"{0}6. ADDITIONAL LICENSES{1}6.1 SQLite.Net{2}",
                our + nl + nl, nl + nl, nl + nl + SQLITE_NET_EULA);
        }

        const String eulakey = "EULA_AgreedVersion";

        public static void Display(Page root, bool check = true)
        {
            var i = DependencyService.Get<IAppInfo>(DependencyFetchTarget.GlobalInstance);
            Object pv;
            bool has = Application.Current.Properties.TryGetValue(eulakey, out pv) && pv.Equals(i.AppVersion);
            if (!check || !has)
            {
                root.DisplayAlert(
                    "EULA Agreement", 
                    BuildEULA(i), 
                    has ? "Close" : "Agree").ContinueWith(tb =>
                {
                    if (!has) Application.Current.Properties.Add(eulakey, i.AppVersion);
                });
            }
        }
    }
}
