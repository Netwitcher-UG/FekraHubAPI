// Next Imports
import { NextResponse } from 'next/server'

// Third-party Imports
import Negotiator from 'negotiator'
import { match as matchLocale } from '@formatjs/intl-localematcher'

// Config Imports
import { i18n } from '@configs/i18n'

// Util Imports
import { getLocalizedUrl, isUrlMissingLocale } from '@/utils/i18n'
import { ensurePrefix } from '@/utils/string'

// Constants
const HOME_PAGE_URL = '/dashboards/crm'

const getLocale = request => {
  // Try to get locale from URL
  const urlLocale = i18n.locales.find(locale => request.nextUrl.pathname.startsWith(`/${locale}`))

  if (urlLocale) return urlLocale

  // Negotiator expects plain object so we need to transform headers
  const negotiatorHeaders = {}

  request.headers.forEach((value, key) => (negotiatorHeaders[key] = value))

  // @ts-ignore locales are readonly
  const locales = i18n.locales

  // Use negotiator and intl-localematcher to get best locale
  const languages = new Negotiator({ headers: negotiatorHeaders }).languages(locales)
  const locale = matchLocale(languages, locales, i18n.defaultLocale)

  return locale
}

const localizedRedirect = (url, locale, request) => {
  let _url = url
  const isLocaleMissing = isUrlMissingLocale(_url)

  if (isLocaleMissing) {
    // e.g. incoming request is /products
    // The new URL is now /en/products
    _url = getLocalizedUrl(_url, locale ?? i18n.defaultLocale)
  }

  let _basePath = process.env.BASEPATH ?? ''

  _basePath = _basePath.replace('demo-1', request.headers.get('X-server-header') ?? 'demo-1')
  _url = ensurePrefix(_url, `${_basePath ?? ''}`)
  const redirectUrl = new URL(_url, request.url).toString()

  return NextResponse.redirect(redirectUrl)
}

export default async function middleware(request) {
  // Get locale from request headers
  const locale = getLocale(request)
  const pathname = request.nextUrl.pathname

  // Private routes (Routes that can only be accessed by logged in users)
  const privateRoutes = ['private-route']

  // Check if user is logged in
  const token = request.nextauth?.token // Safely access token using optional chaining (?.)

  // If the user is trying to access a private route and is not logged in, redirect to the home page
  if (!token && privateRoutes.some(route => pathname.endsWith(route))) {
    return localizedRedirect(HOME_PAGE_URL, locale, request)
  }

  // If pathname already contains a locale, return next() else redirect with localized URL
  return isUrlMissingLocale(pathname) ? localizedRedirect(pathname, locale, request) : NextResponse.next()
}

// Matcher Config
export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - all items inside the public folder
     *    - images (public images)
     *    - next.svg (Next.js logo)
     *    - vercel.svg (Vercel logo)
     */
    '/((?!api|_next/static|_next/image|favicon.ico|.+?/hook-examples|.+?/menu-examples|images|next.svg|vercel.svg).*)'
  ]
}
