/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../KnockoutJS.d.ts'/>
///<reference path='../../JQuery.d.ts' />

/*
 *  Parallax knockout binding extension
 *
 *
 * USAGE EXAMPLE:
 * parallax: {
 *          parallaxBackgroundElement: ".backgroundDOMElement",
 *          parallaxScrollElement: ".scrollingDOMElement",
 *          parallaxSpeed: 3,
 *          parallaxReverse: true
 *        }
 *
 * @param {string} parallaxBackgroundElement (Required) Specifies the selector (class, id, or tagName) of the DOM element on which to apply the parallax effect.
 * @param {number} parallaxScrollElement (Optional) Specifies the selector (class, id, or tagName) of the DOM element on which to bind the scroll event handler.
 *                 (defaults to the current bind element).
 * @param {number} parallaxSpeed (Optional) Sets the scroll speed of the background image container relative to the normal scroll speed; the higher this number, the slower the background scroll speed.
 *                 (default is 5).
 * @param {boolean} parallaxReverse (Optional) Reverses the direction of the parallax. One instance to use this is if the background element is scrollable rather than statically positioned.
 *
 * HTML MARKUP:
 * <body>
 *     <div class="backgroundImage"></div>
 *     <div class="container" data-bind="parallax: {parallaxBackgroundElement: '.backgroundImage', parallaxScrollElement: '.container section[role=main]', parallaxSpeed: 8 }">
 *         <section role="main">
 *             <!-- contents inside of this element are scrollable -->
 *         </section>
 *     </div>
 * </body>
 *
 */

/* enumeration for tracking the current orientation of the device (landscape = 1, portrait = 2).
These numbers are based on the Windows.Graphics.Display.DisplayOrientation enumerations */
enum orientationEnum {
    landscape = 1,
    portrait = 2
};

ko.bindingHandlers.parallax = (() => {
    "use strict";

    var DEFAULT_PARALLAX_SPEED = 3;

    /**
    * Initializes the settings for parallax depending on the orientation of the device.
    *
    * @param element - the scrollable HTML element which contains the parallax element
    * @param value - the options passed into the knockout binding
    * @param orientation - an integer representing the current device orientation (see orientationEnum comments)
    * @param $parallaxBackground - an HTML element on which to create the parallax effect
    */
    function applyParallax(element, value, orientation, $parallaxBackground) {
        var isOrientationLandscape = orientation === orientationEnum.landscape;
        // sets the controlling scroll DOM element if it is passed along with the binding, otherwise defaults to the element being bound.
        var parallaxScrollElement = value.parallaxScrollElement ? value.parallaxScrollElement : element;

        var direction;
        var backgroundScrollDirection;
        if (isOrientationLandscape) {
            direction = (!Commerce.CSSHelpers.isRightToLeft() && value.parallaxReverse) ||
                (Commerce.CSSHelpers.isRightToLeft() && !value.parallaxReverse) ? 1 : -1;
            backgroundScrollDirection = "background-position-x";
        } else {
            direction = 1;
            backgroundScrollDirection = "background-position-y";
        }

        $(parallaxScrollElement).scroll(function (event) {
            var scrollPosition = isOrientationLandscape ? $(this).scrollLeft() : $(this).scrollTop();

            // the larger the value, the slower the background scrolling speed
            var scrollTo = scrollPosition / (value.parallaxSpeed ? value.parallaxSpeed : DEFAULT_PARALLAX_SPEED);

            $parallaxBackground.css(backgroundScrollDirection, Math.floor(direction * scrollTo) + "px");
        });
    }

    return {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            // Uses the height/width ratio to determine the initial orientation of the device.
            var orientation = window.innerHeight > window.innerWidth ? orientationEnum.portrait : orientationEnum.landscape;
            var value = valueAccessor();
            var $parallaxBackground;
            // the parallax binding must pass a value for parallaxBackgroundElement
            if (value.parallaxBackgroundElement) {
                $parallaxBackground = $(value.parallaxBackgroundElement);
            } else {
                Commerce.RetailLogger.viewsControlsKnockoutParallaxBackgroundElementRequired();
                return;
            }

            Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, () => {
                // Not all devices have completed rendering when this eventhandler is triggered, so rather than calculating
                // height/width ratio (which will be incorrect), the orientation is simply toggled here.
                orientation = Commerce.ApplicationContext.Instance.tillLayoutProxy.orientation == "Portrait"? orientationEnum.portrait : orientationEnum.landscape;

                // reset window scroll position to prevent the page from rendering strangely due to CSS flex if it tries to
                // re-orient the device with a partial scroll
                $(element).scrollTop(0);
                $(element).scrollLeft(0);

                // unbind the previous scroll handler to prevent multiple bindings being made when switching orientation
                $parallaxBackground.unbind("scroll");

                applyParallax(element, value, orientation, $parallaxBackground);
            });

            applyParallax(element, valueAccessor(), orientation, $parallaxBackground);
        }
    };
})();

