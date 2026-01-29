using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace C8.eServices.Mvc.Helpers
{
    public class PropertyHelper
    {
        /// <summary>
        /// Orders the properties according to the data annotations - column order.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public static List<PropertyInfo> OrderProperties( PropertyInfo[] info )
        {
            var props = new List<PropertyInfo>( info.Count() );

            for ( int i = 0; i < info.Count(); i++ ) props.Add( null );

            foreach ( var i in info )
            {
                var order = i.GetCustomAttributes( typeof( ColumnAttribute ), true ).FirstOrDefault();

                if ( order != null )
                    props.Insert( ( ( ColumnAttribute ) order ).Order, i );
            }

            return props;
        }
    }
}