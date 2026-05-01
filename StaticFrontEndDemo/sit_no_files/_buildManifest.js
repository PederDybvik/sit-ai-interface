self.__BUILD_MANIFEST = {
  "/404": [
    "static/chunks/0s4l7_vqaw4ba.js"
  ],
  "/[[...slug]]": [
    "static/chunks/0gfhigvqrvtto.js"
  ],
  "/_error": [
    "static/chunks/127zd259hd9dv.js"
  ],
  "/login-callback": [
    "static/chunks/07fi2dg8rm.s4.js"
  ],
  "/logout-callback": [
    "static/chunks/0~s6_0o0x_4om.js"
  ],
  "/profile": [
    "static/chunks/0o4ezpva9gpp0.js"
  ],
  "/search": [
    "static/chunks/020bplmlt-sgk.js"
  ],
  "__rewrites": {
    "afterFiles": [
      {
        "source": "/:nextInternalLocale(no|en)/sok",
        "destination": "/:nextInternalLocale/search"
      },
      {
        "source": "/:nextInternalLocale(no|en)/studio-(.*)",
        "destination": "/:nextInternalLocale/studio"
      }
    ],
    "beforeFiles": [],
    "fallback": []
  },
  "sortedPages": [
    "/404",
    "/_app",
    "/_error",
    "/api/exit-preview",
    "/api/fetch-affy-feed",
    "/api/fetch-alerts",
    "/api/fetch-articles",
    "/api/fetch-settings",
    "/api/form-submit",
    "/api/ping",
    "/api/ping-cron",
    "/api/preview",
    "/api/revalidate",
    "/login-callback",
    "/logout-callback",
    "/profile",
    "/search",
    "/[[...slug]]"
  ]
};self.__BUILD_MANIFEST_CB && self.__BUILD_MANIFEST_CB()