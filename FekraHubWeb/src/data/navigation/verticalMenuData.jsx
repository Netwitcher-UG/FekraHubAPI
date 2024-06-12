const verticalMenuData = (dictionary, params) => [
  // This is how you will normally render submenu
  {
    label: dictionary['navigation'].dashboards,
    icon: 'ri-home-smile-line',
    children: [
      // This is how you will normally render menu item
      {
        label: dictionary['navigation'].crm,
        icon: 'ri-circle-line',
        href: '/dashboards/crm'
      }
    ]
  },

  // This is how you will normally render menu section
  {
    label: dictionary['navigation'].Pages,
    isSection: true,
    children: [
      {
        label: dictionary['navigation'].children,
        icon: 'ri-checkbox-multiple-line',

        children: [
          {
            label: dictionary['navigation'].add,
            icon: 'ri-checkbox-multiple-line',
            href: '/pages/parents/children/create-student'
          },
          {
            label: dictionary['navigation'].view,
            icon: 'ri-checkbox-multiple-line',
            href: '/pages/parents/children/view-students'
          }
        ]
      }
    ]
  }
]

export default verticalMenuData
